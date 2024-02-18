using H4H.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace H4H.Domain.Interfaces
{
    public interface IOrganizationRepository
    {
        Task<Organization> GetByIdAsync(int id);
        Task<List<Organization>> GetAllAsync();
        Task AddAsync(Organization organization);
        Task UpdateAsync(Organization organization);
        Task DeleteAsync(Organization organization);
    }
}
