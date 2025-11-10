using H4H.Application.DTOs;
using H4H.Application.Interfaces;
using H4H.Domain.Enums;
using H4H.Presentation.Web.Services;

namespace H4H.Presentation.Web.Components.Services;

/// <summary>
/// Hybrid chat service that uses API client for remote calls
/// with fallback to direct service for local development.
/// </summary>
public interface IHybridChatService
{
    Task<ChatSessionDto> CreateSessionAsync(Guid userId, ChatLanguage preferredLanguage);
    Task<List<ChatSessionDto>> GetUserSessionsAsync(Guid userId);
    Task<List<ChatMessageDto>> GetSessionMessagesAsync(Guid sessionId);
    Task<ChatMessageDto> SendMessageAsync(Guid sessionId, string content);
    Task UpdateSessionLanguageAsync(Guid sessionId, ChatLanguage language);
}

public class HybridChatService : IHybridChatService
{
    private readonly IApiClient _apiClient;
    private readonly IChatService _directService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<HybridChatService> _logger;

    public HybridChatService(
        IApiClient apiClient,
        IChatService directService,
        IConfiguration configuration,
        ILogger<HybridChatService> logger)
    {
        _apiClient = apiClient;
        _directService = directService;
        _configuration = configuration;
        _logger = logger;
    }

    private bool UseApiClient => !string.IsNullOrEmpty(_configuration["ApiSettings:BaseUrl"]);

    public async Task<ChatSessionDto> CreateSessionAsync(Guid userId, ChatLanguage preferredLanguage)
    {
        if (UseApiClient)
        {
            try
            {
                var session = await _apiClient.CreateChatSessionAsync(preferredLanguage);
                if (session != null) return session;
                
                _logger.LogWarning("API client returned null, falling back to direct service");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling API, falling back to direct service");
            }
        }

        return await _directService.CreateSessionAsync(userId, preferredLanguage);
    }

    public async Task<List<ChatSessionDto>> GetUserSessionsAsync(Guid userId)
    {
        if (UseApiClient)
        {
            try
            {
                var sessions = await _apiClient.GetUserChatSessionsAsync(userId);
                if (sessions != null) return sessions;
                
                _logger.LogWarning("API client returned null, falling back to direct service");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling API, falling back to direct service");
            }
        }

        return await _directService.GetUserSessionsAsync(userId);
    }

    public async Task<List<ChatMessageDto>> GetSessionMessagesAsync(Guid sessionId)
    {
        if (UseApiClient)
        {
            try
            {
                var messages = await _apiClient.GetChatMessagesAsync(sessionId);
                if (messages != null) return messages;
                
                _logger.LogWarning("API client returned null, falling back to direct service");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling API, falling back to direct service");
            }
        }

        return await _directService.GetSessionMessagesAsync(sessionId);
    }

    public async Task<ChatMessageDto> SendMessageAsync(Guid sessionId, string content)
    {
        if (UseApiClient)
        {
            try
            {
                var message = await _apiClient.SendMessageAsync(sessionId, content);
                if (message != null) return message;
                
                _logger.LogWarning("API client returned null, falling back to direct service");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling API, falling back to direct service");
            }
        }

        return await _directService.SendMessageAsync(sessionId, content);
    }

    public async Task UpdateSessionLanguageAsync(Guid sessionId, ChatLanguage language)
    {
        if (UseApiClient)
        {
            try
            {
                // API doesn't have this endpoint yet, use direct service
                await _directService.UpdateSessionLanguageAsync(sessionId, language);
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating session language");
            }
        }

        await _directService.UpdateSessionLanguageAsync(sessionId, language);
    }
}
