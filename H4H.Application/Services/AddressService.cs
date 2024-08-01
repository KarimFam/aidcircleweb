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

            public AddressService(IAddressRepository addressRepository)
            {
                _addressRepository = addressRepository;
            }

            public async Task<IEnumerable<Address>> GetAllAddressesAsync()
            {
                return await _addressRepository.GetAllAsync();
            }

            public async Task<Address> GetAddressByIdAsync(Guid addressId) // Change to Guid to match AddressId type
            {
                return await _addressRepository.GetByIdAsync(addressId);
            }

            public async Task AddAddressAsync(Address address)
            {
                await _addressRepository.AddAsync(address);
            }

            public async Task UpdateAddressAsync(Address address)
            {
                await _addressRepository.UpdateAsync(address);
            }

            public async Task DeleteAddressAsync(Guid id) // Change to Guid to match AddressId type
            {
                var address = await _addressRepository.GetByIdAsync(id);
                if (address != null)
                {
                    await _addressRepository.DeleteAsync(address);
                }
            }

            // Additional methods specific to Address operations
            public async Task<IEnumerable<Address>> GetAddressesByUserIdAsync(Guid userId)
            {
                return await _addressRepository.GetAddressByUserIdAsync(userId);
            }

            public async Task<IEnumerable<Address>> GetAddressesByOrganizationIdAsync(Guid organizationId)
            {
                return await _addressRepository.GetAddressByOrganizationIdAsync(organizationId);
            }

            public async Task<IEnumerable<Address>> GetAddressesByItemIdAsync(Guid itemId)
            {
                return await _addressRepository.GetAddressesByItemIdAsync(itemId);
            }

            public async Task<IEnumerable<Address>> GetAddressesByOrderIdAsync(Guid orderId)
            {
                return await _addressRepository.GetAddressByOrderIdAsync(orderId);
            }
        }
    }

