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

            public async Task<Address> GetAddressByIdAsync(Guid AddressId) // Change to Guid to match AddressId type
            {
                return await _addressRepository.GetByIdAsync(AddressId);
            }

            public async Task AddAddressAsync(Address address)
            {
                await _addressRepository.AddAsync(address);
            }

            public async Task UpdateAddressAsync(Address address)
            {
                await _addressRepository.UpdateAsync(address);
            }

            public async Task DeleteAddressAsync(Guid AddessId) // Change to Guid to match AddressId type
            {
                var address = await _addressRepository.GetByIdAsync(AddessId);
                if (address != null)
                {
                    await _addressRepository.DeleteAsync(address);
                }
            }

            // Additional methods specific to Address operations
            public async Task<IEnumerable<Address>> GetAddressesByUserIdAsync(Guid UserId)
            {
                return await _addressRepository.GetAddressByUserIdAsync(UserId);
            }

            public async Task<IEnumerable<Address>> GetAddressesByOrganizationIdAsync(Guid OrganizationId)
            {
                return await _addressRepository.GetAddressByOrganizationIdAsync(OrganizationId);
            }

            public async Task<IEnumerable<Address>> GetAddressesByItemIdAsync(Guid ItemId)
            {
                return await _addressRepository.GetAddressesByItemIdAsync(ItemId);
            }

            public async Task<IEnumerable<Address>> GetAddressesByOrderIdAsync(Guid OrderId)
            {
                return await _addressRepository.GetAddressByOrderIdAsync(OrderId);
            }
        }
    }

