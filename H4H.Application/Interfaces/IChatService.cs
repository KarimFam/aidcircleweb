using H4H.Application.DTOs;
using H4H.Domain.Enums;

namespace H4H.Application.Interfaces
{
    public interface IChatService
    {
        Task<ChatSessionDto> CreateSessionAsync(Guid userId, ChatLanguage preferredLanguage);
        Task<ChatSessionDto?> GetSessionByIdAsync(Guid chatSessionId);
        Task<List<ChatSessionDto>> GetUserSessionsAsync(Guid userId);
        Task<ChatMessageDto> SendMessageAsync(Guid chatSessionId, string message);
        Task<List<ChatMessageDto>> GetSessionMessagesAsync(Guid chatSessionId);
        Task DeleteSessionAsync(Guid chatSessionId);
        Task<ChatSessionDto> UpdateSessionLanguageAsync(Guid chatSessionId, ChatLanguage language);
    }
}
