using H4H.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace H4H.Domain.Interfaces
{
    public interface IItemRepository
    {
        Task<Item> GetByIdAsync(int id);
        Task<List<Item>> GetAllAsync();
        Task AddAsync(Item entity);
        Task UpdateAsync(Item entity);
        Task DeleteAsync(Item entity);
    }
}
