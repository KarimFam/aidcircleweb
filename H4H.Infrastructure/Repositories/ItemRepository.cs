using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using H4H.Domain.Entities;
using H4H.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using H4H.Infrastructure.Data.Contexts;

namespace H4H.Infrastructure.Repositories
{
    public class ItemRepository : IItemRepository
    {
        private readonly H4HDbContext _context;

        public ItemRepository(H4HDbContext context)
        {
            _context = context;
        }

        public async Task<Item> GetByIdAsync(Guid ItemId)
        {
            return await _context.Items.FindAsync(ItemId);
        }

        public async Task<List<Item>> GetLatestItemsAsync(DateTime fromDate)
        {
            return await _context.Items
                                 .Where(item => item.CreatedDate >= fromDate)
                                 .ToListAsync();
        }

        public async Task<List<Item>> GetAllAsync()
        {
            return await _context.Items.ToListAsync();
        }

        public async Task AddAsync(Item entity)
        {
            await _context.Items.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Item entity)
        {
            _context.Items.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Item entity)
        {
            _context.Items.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}
