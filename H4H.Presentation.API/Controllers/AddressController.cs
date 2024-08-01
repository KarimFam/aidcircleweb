using Microsoft.AspNetCore.Mvc;
using H4H.Application.Interfaces;
using H4H.Domain.Entities;
using System.Threading.Tasks;

namespace H4H.Presentation.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AddressController : ControllerBase
    {
        private readonly IAddressService _addressService;

        public AddressController(IAddressService addressService)
        {
            _addressService = addressService;
        }

        // GET: api/address
        [HttpGet]
        public async Task<IActionResult> GetAllAddresses()
        {
            var addresses = await _addressService.GetAllAddressesAsync();
            return Ok(addresses);
        }

        // GET: api/address/{AddressId}
        [HttpGet("{AddressId}")]
        public async Task<IActionResult> GetAddressById(Guid AddressId)
        {
            var address = await _addressService.GetAddressByIdAsync(AddressId);
            if (address == null)
            {
                return NotFound($"Address with ID {AddressId} not found.");
            }
            return Ok(address);
        }

        // POST: api/address
        [HttpPost]
        public async Task<IActionResult> CreateAddress([FromBody] Address address)
        {
            if (address == null)
            {
                return BadRequest("Address is null.");
            }

            await _addressService.AddAddressAsync(address);
            return CreatedAtAction(nameof(GetAddressById), new { AddressId = address.AddressId }, address);
        }

        // PUT: api/address/{AddressId}
        [HttpPut("{AddressId}")]
        public async Task<IActionResult> UpdateAddress(Guid AddressId, [FromBody] Address address)
        {
            if (address == null || AddressId != address.AddressId)
            {
                return BadRequest("Address is null or ID mismatch.");
            }

            await _addressService.UpdateAddressAsync(address);
            return NoContent();
        }

        // DELETE: api/address/{AddressId}
        [HttpDelete("{AddressId}")]
        public async Task<IActionResult> DeleteAddress(Guid AddressId)
        {
            var address = await _addressService.GetAddressByIdAsync(AddressId);
            if (address == null)
            {
                return NotFound($"Address with ID {AddressId} not found.");
            }

            await _addressService.DeleteAddressAsync(AddressId);
            return NoContent();
        }

        // GET: api/address/user/{UserId}
        [HttpGet("user/{UserId}")]
        public async Task<IActionResult> GetAddressesByUserId(Guid UserId)
        {
            var addresses = await _addressService.GetAddressesByUserIdAsync(UserId);
            return Ok(addresses);
        }

        // GET: api/address/organization/{OrganizationId}
        [HttpGet("organization/{OrganizationId}")]
        public async Task<IActionResult> GetAddressesByOrganizationId(Guid OrganizationId)
        {
            var addresses = await _addressService.GetAddressesByOrganizationIdAsync(OrganizationId);
            return Ok(addresses);
        }

        // GET: api/address/item/{ItemId}
        [HttpGet("item/{ItemId}")]
        public async Task<IActionResult> GetAddressesByItemId(Guid ItemId)
        {
            var addresses = await _addressService.GetAddressesByItemIdAsync(ItemId);
            return Ok(addresses);
        }

        // GET: api/address/order/{OrderId}
        [HttpGet("order/{OrderId}")]
        public async Task<IActionResult> GetAddressesByOrderId(Guid OrderId)
        {
            var addresses = await _addressService.GetAddressesByOrderIdAsync(OrderId);
            return Ok(addresses);
        }
    }
}
