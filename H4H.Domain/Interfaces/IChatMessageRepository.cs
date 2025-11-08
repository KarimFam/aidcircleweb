using H4H.Domain.Entities;

namespace H4H.Domain.Interfaces
{
    public interface IChatMessageRepository
    {
        Task<ChatMessage> GetByIdAsync(Guid chatMessageId);
        Task<List<ChatMessage>> GetAllAsync();
        Task<List<ChatMessage>> GetBySessionIdAsync(Guid chatSessionId);
        Task AddAsync(ChatMessage entity);
        Task UpdateAsync(ChatMessage entity);
        Task DeleteAsync(ChatMessage entity);
    }
}
