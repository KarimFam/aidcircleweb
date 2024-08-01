using H4H.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace H4H.Application.Interfaces
{
    public interface IAddressService
    {
        Task<IEnumerable<Address>> GetAllAddressesAsync();
        Task<Address> GetAddressByIdAsync(Guid addressId); // Change to Guid to match AddressId type
        Task AddAddressAsync(Address address);
        Task UpdateAddressAsync(Address address);
        Task DeleteAddressAsync(Guid addressId); // Change to Guid to match AddressId type

        // Additional methods specific to Address operations
        Task<IEnumerable<Address>> GetAddressesByUserIdAsync(Guid userId);
        Task<IEnumerable<Address>> GetAddressesByOrganizationIdAsync(Guid organizationId);
        Task<IEnumerable<Address>> GetAddressesByItemIdAsync(Guid itemId);
        Task<IEnumerable<Address>> GetAddressesByOrderIdAsync(Guid orderId);
    }
}
