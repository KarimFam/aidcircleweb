using Microsoft.AspNetCore.Mvc;
using H4H.Application.Interfaces;
using H4H.Domain.Entities;
using H4H.Presentation.API.Data;
using System;
using System.Threading.Tasks;
using AutoMapper;

namespace H4H.Presentation.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ItemController : ControllerBase
    {
        private readonly IItemService _itemService;
        private readonly IMapper _mapper;

        public ItemController(IItemService itemService, IMapper mapper)
        {
            _itemService = itemService;
            _mapper = mapper;
        }

        // GET: api/item
        [HttpGet]
        public async Task<IActionResult> GetAllItems()
        {
            var items = await _itemService.GetAllItemsAsync();
            var itemDtos = _mapper.Map<IEnumerable<ItemDto>>(items);
            return Ok(itemDtos);
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
            var itemDto = _mapper.Map<ItemDto>(item);
            return Ok(itemDto);
        }

        // POST: api/item
        [HttpPost]
        public async Task<IActionResult> CreateItem([FromBody] ItemDto itemDto)
        {
            if (itemDto == null)
            {
                return BadRequest("Item is null.");
            }

            var item = _mapper.Map<Item>(itemDto);
            await _itemService.AddItemAsync(item);
            return CreatedAtAction(nameof(GetItemById), new { ItemId = item.ItemId }, itemDto);
        }

        // PUT: api/item/{ItemId}
        [HttpPut("{ItemId}")]
        public async Task<IActionResult> UpdateItem(Guid ItemId, [FromBody] ItemDto itemDto)
        {
            if (itemDto == null)
            {
                return BadRequest("Item is null.");
            }

            var item = await _itemService.GetItemByIdAsync(ItemId);
            if (item == null)
            {
                return NotFound($"Item with ID {ItemId} not found.");
            }

            _mapper.Map(itemDto, item);
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