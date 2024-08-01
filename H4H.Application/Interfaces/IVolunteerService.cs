using H4H.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace H4H.Application.Interfaces
{
    public interface IVolunteerService
    {
        Task<IEnumerable<Volunteer>> GetAllVolunteersAsync();
        Task<Volunteer> GetVolunteerByIdAsync(Guid volunteerId); // Change to Guid to match VolunteerId type
        Task AddVolunteerAsync(Volunteer volunteer);
        Task UpdateVolunteerAsync(Volunteer volunteer);
        Task DeleteVolunteerAsync(Guid volunteerId); // Change to Guid to match VolunteerId type
    }
}
