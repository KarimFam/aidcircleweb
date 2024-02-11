using H4H.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace H4H.Domain.Interfaces
{
    public interface IVolunteerRepository
    {
        Task<Volunteer> GetByIdAsync(int id);
        Task<List<Volunteer>> GetAllAsync();
        Task AddAsync(Volunteer entity);
        Task UpdateAsync(Volunteer entity);
        Task DeleteAsync(Volunteer entity);
    }
}
