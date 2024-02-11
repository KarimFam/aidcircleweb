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
    }
}
