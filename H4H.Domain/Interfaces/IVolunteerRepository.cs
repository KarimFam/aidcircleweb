using H4H.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace H4H.Domain.Interfaces
{
    public interface IVolunteerRepository
    {
        Task<Volunteer> GetByIdAsync(Guid VolunteerId);
        Task<List<Volunteer>> GetAllAsync();
        Task AddAsync(Volunteer entity);
        Task UpdateAsync(Volunteer entity);
        Task DeleteAsync(Volunteer entity);
    }
}
