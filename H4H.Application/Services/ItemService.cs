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
        private readonly ILogger<ItemService> _logger;

        public ItemService(IItemRepository itemRepository, ILogger<ItemService> logger)
        {
            _itemRepository = itemRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<Item>> GetAllItemsAsync()
        {
            try
            {
                return await _itemRepository.GetAllAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving items");
                throw;
            }
        }

        public async Task<Item> GetItemByIdAsync(int id)
        {
            try
            {
                return await _itemRepository.GetByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while retrieving item with ID {id}");
                throw;
            }
        }

        public async Task AddItemAsync(Item item)
        {
            try
            {
                await _itemRepository.AddAsync(item);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while adding a new item");
                throw;
            }
        }

        public async Task UpdateItemAsync(Item item)
        {
            try
            {
                await _itemRepository.UpdateAsync(item);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while updating item with ID {item.ItemId}");
                throw;
            }
        }

        public async Task DeleteItemAsync(int id)
        {
            try
            {
                var item = await _itemRepository.GetByIdAsync(id);
                if (item != null)
                {
                    await _itemRepository.DeleteAsync(item);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while deleting item with ID {id}");
                throw;
            }
        }
    }
}
