using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;
using H4H.Application.DTOs;
using H4H.Domain.Enums;

namespace H4H.Presentation.Web.Services;

/// <summary>
/// Implementation of API client with dual authentication support:
/// - Azure AD B2C bearer tokens for user-initiated operations
/// - API key for service-to-service operations (background tasks)
/// All secrets loaded from Azure Key Vault via configuration.
/// </summary>
public class ApiClient : IApiClient
{
    private readonly HttpClient _httpClient;
    private readonly AuthenticationStateProvider _authStateProvider;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ApiClient> _logger;

    public ApiClient(
        HttpClient httpClient,
        AuthenticationStateProvider authStateProvider,
        IConfiguration configuration,
        ILogger<ApiClient> logger)
    {
        _httpClient = httpClient;
        _authStateProvider = authStateProvider;
        _configuration = configuration;
        _logger = logger;

        // Base URL from configuration (different for local dev vs production)
        var apiBaseUrl = _configuration["ApiSettings:BaseUrl"] ?? throw new InvalidOperationException("ApiSettings:BaseUrl not configured");
        _httpClient.BaseAddress = new Uri(apiBaseUrl);
        _httpClient.Timeout = TimeSpan.FromSeconds(30);
    }

    /// <summary>
    /// Adds authentication header to the request.
    /// Priority: Azure AD B2C bearer token > API key fallback.
    /// </summary>
    private async Task AddAuthHeaderAsync(bool forceApiKey = false)
    {
        // Clear existing auth headers
        _httpClient.DefaultRequestHeaders.Authorization = null;
        _httpClient.DefaultRequestHeaders.Remove("X-API-Key");

        if (!forceApiKey)
        {
            try
            {
                // Try Azure AD B2C token first (for user-initiated operations)
                var authState = await _authStateProvider.GetAuthenticationStateAsync();
                var user = authState.User;

                if (user.Identity?.IsAuthenticated == true)
                {
                    // Get access token from claims
                    var accessToken = user.Claims.FirstOrDefault(c => c.Type == "access_token")?.Value;
                    
                    if (!string.IsNullOrEmpty(accessToken))
                    {
                        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                        _logger.LogDebug("Using Azure AD B2C bearer token for authentication");
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to get Azure AD B2C token, falling back to API key");
            }
        }

        // Fallback to API key (for service operations or when B2C token unavailable)
        var apiKey = _configuration["ApiSettings:ApiKey"];
        if (!string.IsNullOrEmpty(apiKey))
        {
            _httpClient.DefaultRequestHeaders.Add("X-API-Key", apiKey);
            _logger.LogDebug("Using API key for authentication");
        }
        else
        {
            _logger.LogWarning("No authentication method available (no B2C token or API key)");
        }
    }

    #region HTTP Helper Methods

    private async Task<T?> GetAsync<T>(string endpoint, bool forceApiKey = false)
    {
        try
        {
            await AddAuthHeaderAsync(forceApiKey);
            var response = await _httpClient.GetAsync(endpoint);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("GET {Endpoint} failed with status {StatusCode}: {Reason}", endpoint, response.StatusCode, response.ReasonPhrase);
                return default;
            }

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling GET {Endpoint}", endpoint);
            return default;
        }
    }

    private async Task<T?> PostAsync<T>(string endpoint, object? data = null, bool forceApiKey = false)
    {
        try
        {
            await AddAuthHeaderAsync(forceApiKey);
            
            HttpResponseMessage response;
            if (data != null)
            {
                var json = JsonSerializer.Serialize(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                response = await _httpClient.PostAsync(endpoint, content);
            }
            else
            {
                response = await _httpClient.PostAsync(endpoint, null);
            }

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("POST {Endpoint} failed with status {StatusCode}: {Reason}", endpoint, response.StatusCode, response.ReasonPhrase);
                return default;
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling POST {Endpoint}", endpoint);
            return default;
        }
    }

    private async Task<T?> PutAsync<T>(string endpoint, object data, bool forceApiKey = false)
    {
        try
        {
            await AddAuthHeaderAsync(forceApiKey);
            var json = JsonSerializer.Serialize(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync(endpoint, content);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("PUT {Endpoint} failed with status {StatusCode}: {Reason}", endpoint, response.StatusCode, response.ReasonPhrase);
                return default;
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling PUT {Endpoint}", endpoint);
            return default;
        }
    }

    private async Task<bool> DeleteAsync(string endpoint, bool forceApiKey = false)
    {
        try
        {
            await AddAuthHeaderAsync(forceApiKey);
            var response = await _httpClient.DeleteAsync(endpoint);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("DELETE {Endpoint} failed with status {StatusCode}: {Reason}", endpoint, response.StatusCode, response.ReasonPhrase);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling DELETE {Endpoint}", endpoint);
            return false;
        }
    }

    #endregion

    #region Chat Endpoints

    public async Task<ChatSessionDto?> CreateChatSessionAsync(ChatLanguage preferredLanguage)
    {
        return await PostAsync<ChatSessionDto>("/api/chat/sessions", new { PreferredLanguage = preferredLanguage });
    }

    public async Task<List<ChatSessionDto>> GetUserChatSessionsAsync(Guid userId)
    {
        return await GetAsync<List<ChatSessionDto>>($"/api/chat/users/{userId}/sessions") ?? new List<ChatSessionDto>();
    }

    public async Task<ChatSessionDto?> GetChatSessionAsync(Guid sessionId)
    {
        return await GetAsync<ChatSessionDto>($"/api/chat/sessions/{sessionId}");
    }

    public async Task<ChatMessageDto?> SendMessageAsync(Guid sessionId, string content)
    {
        return await PostAsync<ChatMessageDto>($"/api/chat/sessions/{sessionId}/messages", new { Content = content });
    }

    public async Task<List<ChatMessageDto>> GetChatMessagesAsync(Guid sessionId)
    {
        return await GetAsync<List<ChatMessageDto>>($"/api/chat/sessions/{sessionId}/messages") ?? new List<ChatMessageDto>();
    }

    #endregion

    #region User Endpoints

    public async Task<UserDto?> GetUserAsync(Guid userId)
    {
        return await GetAsync<UserDto>($"/api/users/{userId}");
    }

    public async Task<UserDto?> GetUserByExternalAuthIdAsync(string externalAuthId)
    {
        return await GetAsync<UserDto>($"/api/users/external/{externalAuthId}");
    }

    public async Task<List<UserDto>> GetAllUsersAsync()
    {
        return await GetAsync<List<UserDto>>("/api/users") ?? new List<UserDto>();
    }

    public async Task<UserDto?> CreateUserAsync(UserDto user)
    {
        return await PostAsync<UserDto>("/api/users", user);
    }

    public async Task<UserDto?> UpdateUserAsync(Guid userId, UserDto user)
    {
        return await PutAsync<UserDto>($"/api/users/{userId}", user);
    }

    public async Task<bool> DeleteUserAsync(Guid userId)
    {
        return await DeleteAsync($"/api/users/{userId}");
    }

    #endregion

    #region Volunteer Endpoints

    public async Task<List<VolunteerDto>> GetAllVolunteersAsync()
    {
        return await GetAsync<List<VolunteerDto>>("/api/volunteers") ?? new List<VolunteerDto>();
    }

    public async Task<VolunteerDto?> GetVolunteerAsync(Guid volunteerId)
    {
        return await GetAsync<VolunteerDto>($"/api/volunteers/{volunteerId}");
    }

    public async Task<List<VolunteerDto>> GetOnlineVolunteersAsync()
    {
        return await GetAsync<List<VolunteerDto>>("/api/volunteers/online", forceApiKey: true) ?? new List<VolunteerDto>();
    }

    public async Task<VolunteerDto?> CreateVolunteerAsync(VolunteerDto volunteer)
    {
        return await PostAsync<VolunteerDto>("/api/volunteers", volunteer);
    }

    public async Task<VolunteerDto?> UpdateVolunteerAsync(Guid volunteerId, VolunteerDto volunteer)
    {
        return await PutAsync<VolunteerDto>($"/api/volunteers/{volunteerId}", volunteer);
    }

    public async Task<bool> DeleteVolunteerAsync(Guid volunteerId)
    {
        return await DeleteAsync($"/api/volunteers/{volunteerId}");
    }

    #endregion

    #region Order Endpoints

    public async Task<List<OrderDto>> GetAllOrdersAsync()
    {
        return await GetAsync<List<OrderDto>>("/api/orders") ?? new List<OrderDto>();
    }

    public async Task<List<OrderDto>> GetLatestOrdersAsync()
    {
        return await GetAsync<List<OrderDto>>("/api/orders/latest") ?? new List<OrderDto>();
    }

    public async Task<OrderDto?> GetOrderAsync(Guid orderId)
    {
        return await GetAsync<OrderDto>($"/api/orders/{orderId}");
    }

    public async Task<OrderDto?> CreateOrderAsync(OrderDto order)
    {
        return await PostAsync<OrderDto>("/api/orders", order);
    }

    public async Task<OrderDto?> UpdateOrderAsync(Guid orderId, OrderDto order)
    {
        return await PutAsync<OrderDto>($"/api/orders/{orderId}", order);
    }

    public async Task<bool> DeleteOrderAsync(Guid orderId)
    {
        return await DeleteAsync($"/api/orders/{orderId}");
    }

    #endregion

    #region Item Endpoints

    public async Task<List<ItemDto>> GetAllItemsAsync()
    {
        return await GetAsync<List<ItemDto>>("/api/items") ?? new List<ItemDto>();
    }

    public async Task<ItemDto?> GetItemAsync(Guid itemId)
    {
        return await GetAsync<ItemDto>($"/api/items/{itemId}");
    }

    public async Task<ItemDto?> CreateItemAsync(ItemDto item)
    {
        return await PostAsync<ItemDto>("/api/items", item);
    }

    public async Task<ItemDto?> UpdateItemAsync(Guid itemId, ItemDto item)
    {
        return await PutAsync<ItemDto>($"/api/items/{itemId}", item);
    }

    public async Task<bool> DeleteItemAsync(Guid itemId)
    {
        return await DeleteAsync($"/api/items/{itemId}");
    }

    #endregion

    #region Organization Endpoints

    public async Task<List<OrganizationDto>> GetAllOrganizationsAsync()
    {
        return await GetAsync<List<OrganizationDto>>("/api/organizations") ?? new List<OrganizationDto>();
    }

    public async Task<OrganizationDto?> GetOrganizationAsync(Guid organizationId)
    {
        return await GetAsync<OrganizationDto>($"/api/organizations/{organizationId}");
    }

    public async Task<OrganizationDto?> CreateOrganizationAsync(OrganizationDto organization)
    {
        return await PostAsync<OrganizationDto>("/api/organizations", organization);
    }

    public async Task<OrganizationDto?> UpdateOrganizationAsync(Guid organizationId, OrganizationDto organization)
    {
        return await PutAsync<OrganizationDto>($"/api/organizations/{organizationId}", organization);
    }

    public async Task<bool> DeleteOrganizationAsync(Guid organizationId)
    {
        return await DeleteAsync($"/api/organizations/{organizationId}");
    }

    #endregion

    #region Address Endpoints

    public async Task<List<AddressDto>> GetAllAddressesAsync()
    {
        return await GetAsync<List<AddressDto>>("/api/addresses") ?? new List<AddressDto>();
    }

    public async Task<AddressDto?> GetAddressAsync(Guid addressId)
    {
        return await GetAsync<AddressDto>($"/api/addresses/{addressId}");
    }

    public async Task<AddressDto?> CreateAddressAsync(AddressDto address)
    {
        return await PostAsync<AddressDto>("/api/addresses", address);
    }

    public async Task<AddressDto?> UpdateAddressAsync(Guid addressId, AddressDto address)
    {
        return await PutAsync<AddressDto>($"/api/addresses/{addressId}", address);
    }

    public async Task<bool> DeleteAddressAsync(Guid addressId)
    {
        return await DeleteAsync($"/api/addresses/{addressId}");
    }

    #endregion

    #region Health Check

    public async Task<bool> CheckHealthAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("/health");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    #endregion
}
