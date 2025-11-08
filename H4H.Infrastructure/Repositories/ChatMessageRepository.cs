using H4H.Domain.Entities;
using H4H.Domain.Interfaces;
using H4H.Infrastructure.Data.Contexts;
using Microsoft.EntityFrameworkCore;

namespace H4H.Infrastructure.Repositories
{
    public class ChatMessageRepository : IChatMessageRepository
    {
        private readonly H4HDbContext _context;

        public ChatMessageRepository(H4HDbContext context)
        {
            _context = context;
        }

        public async Task<ChatMessage> GetByIdAsync(Guid chatMessageId)
        {
            return await _context.ChatMessages
                .Include(cm => cm.ChatSession)
                .FirstOrDefaultAsync(cm => cm.ChatMessageId == chatMessageId);
        }

        public async Task<List<ChatMessage>> GetAllAsync()
        {
            return await _context.ChatMessages.ToListAsync();
        }

        public async Task<List<ChatMessage>> GetBySessionIdAsync(Guid chatSessionId)
        {
            return await _context.ChatMessages
                .Where(cm => cm.ChatSessionId == chatSessionId)
                .OrderBy(cm => cm.CreatedDate)
                .ToListAsync();
        }

        public async Task AddAsync(ChatMessage entity)
        {
            await _context.ChatMessages.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(ChatMessage entity)
        {
            _context.ChatMessages.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(ChatMessage entity)
        {
            _context.ChatMessages.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}
