using H4H.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace H4H.Application.Interfaces
{
    public interface IVolunteerService
    {
        Task<IEnumerable<Volunteer>> GetAllVolunteersAsync();
        Task<Volunteer> GetVolunteerByIdAsync(int id);
        Task AddVolunteerAsync(Volunteer volunteer);
        Task UpdateVolunteerAsync(Volunteer volunteer);
        Task DeleteVolunteerAsync(int id);
    }
}
