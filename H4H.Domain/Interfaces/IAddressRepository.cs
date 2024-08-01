using H4H.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace H4H.Domain.Interfaces
{
    public interface IAddressRepository
    {
        Task<Address> GetByIdAsync(Guid addressId);
        Task<List<Address>> GetAllAsync();
        Task AddAsync(Address entity);
        Task UpdateAsync(Address entity);
        Task DeleteAsync(Address entity);

        Task<List<Address>> GetAddressByUserIdAsync(Guid userId);
        //Task<List<Address>> GetAddressesByVolunteerIdAsync(Guid volunteerId);
        Task<List<Address>> GetAddressByOrganizationIdAsync(Guid organizationId);
        Task<List<Address>> GetAddressesByItemIdAsync(Guid itemId);
        Task<List<Address>> GetAddressByOrderIdAsync(Guid orderId);
      
    }
}
