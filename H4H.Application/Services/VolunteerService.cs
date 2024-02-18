using H4H.Domain.Entities;
using H4H.Domain.Interfaces;
using H4H.Application.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace H4H.Application.Services
{
    public class VolunteerService : IVolunteerService
    {
        private readonly IVolunteerRepository _volunteerRepository;
        private readonly ILogger<VolunteerService> _logger;

        public VolunteerService(IVolunteerRepository volunteerRepository, ILogger<VolunteerService> logger)
        {
            _volunteerRepository = volunteerRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<Volunteer>> GetAllVolunteersAsync()
        {
            try
            {
                return await _volunteerRepository.GetAllAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving volunteers");
                throw;
            }
        }

        public async Task<Volunteer> GetVolunteerByIdAsync(int id)
        {
            try
            {
                return await _volunteerRepository.GetByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving volunteer with ID {id}");
                throw;
            }
        }

        public async Task AddVolunteerAsync(Volunteer volunteer)
        {
            try
            {
                await _volunteerRepository.AddAsync(volunteer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding new volunteer");
                throw;
            }
        }

        public async Task UpdateVolunteerAsync(Volunteer volunteer)
        {
            try
            {
                await _volunteerRepository.UpdateAsync(volunteer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating volunteer with ID {volunteer.Id}");
                throw;
            }
        }

        public async Task DeleteVolunteerAsync(int id)
        {
            try
            {
                var volunteer = await _volunteerRepository.GetByIdAsync(id);
                if (volunteer != null)
                {
                    await _volunteerRepository.DeleteAsync(volunteer);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting volunteer with ID {id}");
                throw;
            }
        }
    }
}
