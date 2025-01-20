using H4H.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace H4H.Application.Interfaces
{
    public interface IVolunteerService
    {
        Task<IEnumerable<Volunteer>> GetAllVolunteersAsync();
        Task<List<Volunteer>> GetOnlineVolunteersAsync();
        Task<Volunteer> GetVolunteerByIdAsync(Guid VolunteerId); // Change to Guid to match VolunteerId type
        Task AddVolunteerAsync(Volunteer volunteer);
        Task UpdateVolunteerAsync(Volunteer volunteer);
        Task DeleteVolunteerAsync(Guid VolunteerId); // Change to Guid to match VolunteerId type
    }
}
