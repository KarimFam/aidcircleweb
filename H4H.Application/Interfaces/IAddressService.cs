using H4H.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace H4H.Application.Interfaces
{
    public interface IAddressService
    {
        Task<IEnumerable<Address>> GetAllAddressesAsync();
        Task<Address> GetAddressByIdAsync(Guid AddressId); // Change to Guid to match AddressId type
        Task AddAddressAsync(Address address);
        Task UpdateAddressAsync(Address address);
        Task DeleteAddressAsync(Guid AddressId); // Change to Guid to match AddressId type

        // Additional methods specific to Address operations
        Task<IEnumerable<Address>> GetAddressesByUserIdAsync(Guid UserId);
        Task<IEnumerable<Address>> GetAddressesByOrganizationIdAsync(Guid OrganizationId);
        Task<IEnumerable<Address>> GetAddressesByItemIdAsync(Guid ItemId);
        Task<IEnumerable<Address>> GetAddressesByOrderIdAsync(Guid OrderId);
    }
}
