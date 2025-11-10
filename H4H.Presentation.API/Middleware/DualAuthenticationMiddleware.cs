namespace H4H.Presentation.API.Middleware;

/// <summary>
/// Middleware for dual authentication support:
/// 1. Azure AD B2C bearer tokens (primary for user operations)
/// 2. API key authentication (fallback for service-to-service calls)
/// API key loaded from Azure Key Vault via configuration.
/// </summary>
public class DualAuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<DualAuthenticationMiddleware> _logger;
    private const string API_KEY_HEADER = "X-API-Key";

    public DualAuthenticationMiddleware(RequestDelegate next, ILogger<DualAuthenticationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IConfiguration configuration)
    {
        // Skip authentication for health check and swagger endpoints
        if (context.Request.Path.StartsWithSegments("/health") ||
            context.Request.Path.StartsWithSegments("/swagger") ||
            context.Request.Path.StartsWithSegments("/_framework"))
        {
            await _next(context);
            return;
        }

        // Check if Azure AD B2C bearer token is present
        var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
        if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogDebug("Request authenticated via Azure AD B2C bearer token");
            await _next(context);
            return;
        }

        // Fallback to API key authentication
        if (!context.Request.Headers.TryGetValue(API_KEY_HEADER, out var extractedApiKey))
        {
            _logger.LogWarning("Unauthorized request - missing both Bearer token and API key");
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Unauthorized: Missing authentication credentials");
            return;
        }

        // Validate API key from configuration (loaded from Key Vault)
        var apiKey = configuration["ApiSettings:ApiKey"];
        if (string.IsNullOrEmpty(apiKey))
        {
            _logger.LogError("API key not configured in ApiSettings:ApiKey");
            context.Response.StatusCode = 500;
            await context.Response.WriteAsync("Server configuration error");
            return;
        }

        if (!apiKey.Equals(extractedApiKey.ToString(), StringComparison.Ordinal))
        {
            _logger.LogWarning("Invalid API key provided");
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Unauthorized: Invalid API key");
            return;
        }

        _logger.LogDebug("Request authenticated via API key");
        await _next(context);
    }
}

/// <summary>
/// Extension method for registering the dual authentication middleware.
/// </summary>
public static class DualAuthenticationMiddlewareExtensions
{
    public static IApplicationBuilder UseDualAuthentication(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<DualAuthenticationMiddleware>();
    }
}
