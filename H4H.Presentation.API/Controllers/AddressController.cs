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

        // GET: api/address/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAddressById(int id)
        {
            var address = await _addressService.GetAddressByIdAsync(id);
            if (address == null)
            {
                return NotFound($"Address with ID {id} not found.");
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
            return CreatedAtAction(nameof(GetAddressById), new { id = address.AddressId }, address);
        }

        // PUT: api/address/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAddress(Guid id, [FromBody] Address address)
        {
            if (address == null || id != address.AddressId)
            {
                return BadRequest("Address is null or ID mismatch.");
            }

            await _addressService.UpdateAddressAsync(address);
            return NoContent();
        }

        // DELETE: api/address/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAddress(int id)
        {
            await _addressService.DeleteAddressAsync(id);
            return NoContent();
        }
    }
}
