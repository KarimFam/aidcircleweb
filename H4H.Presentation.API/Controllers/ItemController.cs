using Microsoft.AspNetCore.Mvc;
using H4H.Application.Interfaces;
using H4H.Domain.Entities;
using System.Threading.Tasks;

namespace H4H.Presentation.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ItemController : ControllerBase
    {
        private readonly IItemService _itemService;

        public ItemController(IItemService itemService)
        {
            _itemService = itemService;
        }

        // GET: api/item
        [HttpGet]
        public async Task<IActionResult> GetAllItems()
        {
            var items = await _itemService.GetAllItemsAsync();
            return Ok(items);
        }

        // GET: api/item/{ItemId}
        [HttpGet("{ItemId}")]
        public async Task<IActionResult> GetItemById(Guid ItemId)
        {
            var item = await _itemService.GetItemByIdAsync(ItemId);
            if (item == null)
            {
                return NotFound($"Item with ID {ItemId} not found.");
            }
            return Ok(item);
        }

        // POST: api/item
        [HttpPost]
        public async Task<IActionResult> CreateItem([FromBody] Item item)
        {
            if (item == null)
            {
                return BadRequest("Item is null.");
            }

            await _itemService.AddItemAsync(item);
            return CreatedAtAction(nameof(GetItemById), new { ItemId = item.ItemId }, item);
        }

        // PUT: api/item/{ItemId}
        [HttpPut("{ItemId}")]
        public async Task<IActionResult> UpdateItem(Guid ItemId, [FromBody] Item item)
        {
            if (item == null || ItemId != item.ItemId)
            {
                return BadRequest("Item is null or ID mismatch.");
            }

            await _itemService.UpdateItemAsync(item);
            return NoContent();
        }

        // DELETE: api/item/{ItemId}
        [HttpDelete("{ItemId}")]
        public async Task<IActionResult> DeleteItem(Guid ItemId)
        {
            var item = await _itemService.GetItemByIdAsync(ItemId);
            if (item == null)
            {
                return NotFound($"Item with ID {ItemId} not found.");
            }

            await _itemService.DeleteItemAsync(ItemId);
            return NoContent();
        }
    }
}