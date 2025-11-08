using H4H.Domain.Entities;

namespace H4H.Domain.Interfaces
{
    public interface IChatSessionRepository
    {
        Task<ChatSession> GetByIdAsync(Guid chatSessionId);
        Task<List<ChatSession>> GetAllAsync();
        Task<List<ChatSession>> GetByUserIdAsync(Guid userId);
        Task AddAsync(ChatSession entity);
        Task UpdateAsync(ChatSession entity);
        Task DeleteAsync(ChatSession entity);
    }
}
