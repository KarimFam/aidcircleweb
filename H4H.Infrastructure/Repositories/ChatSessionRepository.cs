using H4H.Domain.Entities;
using H4H.Domain.Interfaces;
using H4H.Infrastructure.Data.Contexts;
using Microsoft.EntityFrameworkCore;

namespace H4H.Infrastructure.Repositories
{
    public class ChatSessionRepository : IChatSessionRepository
    {
        private readonly H4HDbContext _context;

        public ChatSessionRepository(H4HDbContext context)
        {
            _context = context;
        }

        public async Task<ChatSession> GetByIdAsync(Guid chatSessionId)
        {
            return await _context.ChatSessions
                .Include(cs => cs.Messages)
                .Include(cs => cs.User)
                .FirstOrDefaultAsync(cs => cs.ChatSessionId == chatSessionId);
        }

        public async Task<List<ChatSession>> GetAllAsync()
        {
            return await _context.ChatSessions
                .Include(cs => cs.Messages)
                .Include(cs => cs.User)
                .ToListAsync();
        }

        public async Task<List<ChatSession>> GetByUserIdAsync(Guid userId)
        {
            return await _context.ChatSessions
                .Include(cs => cs.Messages)
                .Where(cs => cs.UserId == userId)
                .OrderByDescending(cs => cs.ModifiedDate)
                .ToListAsync();
        }

        public async Task AddAsync(ChatSession entity)
        {
            await _context.ChatSessions.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(ChatSession entity)
        {
            _context.ChatSessions.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(ChatSession entity)
        {
            _context.ChatSessions.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}
