using H4H.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace H4H.Domain.Interfaces
{
    public interface IAddressRepository
    {
        Task<Address> GetByIdAsync(int id);
        Task<List<Address>> GetAllAsync();
        Task AddAsync(Address entity);
        Task UpdateAsync(Address entity);
        Task DeleteAsync(Address entity);

        Task<List<Address>> GetAddressesByUserIdAsync(int userId);
        Task<List<Address>> GetAddressesByVolunteerIdAsync(int volunteerId);
        Task<List<Address>> GetAddressesByOrganizationIdAsync(int organizationId);
        Task<List<Address>> GetAddressesByItemIdAsync(int itemId);
    }
}
