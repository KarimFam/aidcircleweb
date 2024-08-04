using H4H.Application.Interfaces;
using H4H.Domain.Entities;
using H4H.Domain.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace H4H.Application.Services
{
    public class ItemService : IItemService
    {
        private readonly IItemRepository _itemRepository;

        public ItemService(IItemRepository itemRepository)
        {
            _itemRepository = itemRepository;
        }

        public async Task<IEnumerable<Item>> GetAllItemsAsync()
        {
            return await _itemRepository.GetAllAsync();
        }

        public async Task<Item> GetItemByIdAsync(Guid ItemId) // Change to Guid to match ItemId type
        {
            return await _itemRepository.GetByIdAsync(ItemId);
        }

        public async Task AddItemAsync(Item item)
        {
            await _itemRepository.AddAsync(item);
        }

        public async Task UpdateItemAsync(Item item)
        {
            await _itemRepository.UpdateAsync(item);
        }

        public async Task DeleteItemAsync(Guid ItemId) // Change to Guid to match ItemId type
        {
            var item = await _itemRepository.GetByIdAsync(ItemId);
            if (item != null)
            {
                await _itemRepository.DeleteAsync(item);
            }
        }
    }
}
