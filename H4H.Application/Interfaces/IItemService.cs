using H4H.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace H4H.Application.Interfaces
{
    public interface IItemService
    {
        Task<IEnumerable<Item>> GetAllItemsAsync();
        Task<Item> GetItemByIdAsync(Guid ItemId); // Change to Guid to match ItemId type
        Task AddItemAsync(Item item);
        Task UpdateItemAsync(Item item);
        Task DeleteItemAsync(Guid itemId); // Change to Guid to match ItemId type
    }
}
