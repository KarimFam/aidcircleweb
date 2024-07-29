using H4H.Application.Interfaces;
using H4H.Domain.Entities;
using H4H.Domain.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace H4H.Application.Services
{
    public class AddressService : IAddressService
    {
        private readonly IAddressRepository _addressRepository;
        private readonly ILogger<AddressService> _logger;

        public AddressService(IAddressRepository addressRepository, ILogger<AddressService> logger)
        {
            _addressRepository = addressRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<Address>> GetAllAddressesAsync()
        {
            try
            {
                return await _addressRepository.GetAllAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving addresses");
                throw;
            }
        }

        public async Task<Address> GetAddressByIdAsync(int id)
        {
            try
            {
                return await _addressRepository.GetByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while retrieving address with ID {id}");
                throw;
            }
        }

        public async Task AddAddressAsync(Address address)
        {
            try
            {
                await _addressRepository.AddAsync(address);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while adding a new address");
                throw;
            }
        }

        public async Task UpdateAddressAsync(Address address)
        {
            try
            {
                await _addressRepository.UpdateAsync(address);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while updating address with ID {address.AddressId}");
                throw;
            }
        }

        public async Task DeleteAddressAsync(int id)
        {
            try
            {
                var address = await _addressRepository.GetByIdAsync(id);
                if (address != null)
                {
                    await _addressRepository.DeleteAsync(address);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while deleting address with ID {id}");
                throw;
            }
        }
    }
}
