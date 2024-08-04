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
    public class AddressController : ControllerBase
    {
        private readonly IAddressService _addressService;
        private readonly IMapper _mapper;

        public AddressController(IAddressService addressService, IMapper mapper)
        {
            _addressService = addressService;
            _mapper = mapper;
        }

        // GET: api/address
        [HttpGet]
        public async Task<IActionResult> GetAllAddresses()
        {
            var addresses = await _addressService.GetAllAddressesAsync();
            var addressDtos = _mapper.Map<IEnumerable<AddressDto>>(addresses);
            return Ok(addressDtos);
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
            var addressDto = _mapper.Map<AddressDto>(address);
            return Ok(addressDto);
        }

        // POST: api/address
        [HttpPost]
        public async Task<IActionResult> CreateAddress([FromBody] AddressDto addressDto)
        {
            if (addressDto == null)
            {
                return BadRequest("Address is null.");
            }

            var address = _mapper.Map<Address>(addressDto);
            await _addressService.AddAddressAsync(address);
            return CreatedAtAction(nameof(GetAddressById), new { AddressId = address.AddressId }, addressDto);
        }

        // PUT: api/address/{AddressId}
        [HttpPut("{AddressId}")]
        public async Task<IActionResult> UpdateAddress(Guid AddressId, [FromBody] AddressDto addressDto)
        {
            //if (addressDto == null || AddressId != addressDto.AddressId)
            //{
            //    return BadRequest("Address is null or ID mismatch.");
            //}

            var address = _mapper.Map<Address>(addressDto);
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
            var addressDtos = _mapper.Map<IEnumerable<AddressDto>>(addresses);
            return Ok(addressDtos);
        }

        // GET: api/address/organization/{OrganizationId}
        [HttpGet("organization/{OrganizationId}")]
        public async Task<IActionResult> GetAddressesByOrganizationId(Guid OrganizationId)
        {
            var addresses = await _addressService.GetAddressesByOrganizationIdAsync(OrganizationId);
            var addressDtos = _mapper.Map<IEnumerable<AddressDto>>(addresses);
            return Ok(addressDtos);
        }

        // GET: api/address/item/{ItemId}
        [HttpGet("item/{ItemId}")]
        public async Task<IActionResult> GetAddressesByItemId(Guid ItemId)
        {
            var addresses = await _addressService.GetAddressesByItemIdAsync(ItemId);
            var addressDtos = _mapper.Map<IEnumerable<AddressDto>>(addresses);
            return Ok(addressDtos);
        }

        // GET: api/address/order/{OrderId}
        [HttpGet("order/{OrderId}")]
        public async Task<IActionResult> GetAddressesByOrderId(Guid OrderId)
        {
            var addresses = await _addressService.GetAddressesByOrderIdAsync(OrderId);
            var addressDtos = _mapper.Map<IEnumerable<AddressDto>>(addresses);
            return Ok(addressDtos);
        }
    }
}
