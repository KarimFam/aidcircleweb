﻿using H4H.Domain.Entities;
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

        public VolunteerService(IVolunteerRepository volunteerRepository)
        {
            _volunteerRepository = volunteerRepository;
        }

        public async Task<IEnumerable<Volunteer>> GetAllVolunteersAsync()
        {
            return await _volunteerRepository.GetAllAsync();
        }

        public async Task<Volunteer> GetVolunteerByIdAsync(Guid VolunteerId) // Change to Guid to match VolunteerId type
        {
            return await _volunteerRepository.GetByIdAsync(VolunteerId);
        }
        public async Task<List<Volunteer>> GetOnlineVolunteersAsync()
        {
            return await _volunteerRepository.GetOnlineVolunteersAsync();
        }

        public async Task AddVolunteerAsync(Volunteer volunteer)
        {
            await _volunteerRepository.AddAsync(volunteer);
        }

        public async Task UpdateVolunteerAsync(Volunteer volunteer)
        {
            await _volunteerRepository.UpdateAsync(volunteer);
        }

        public async Task DeleteVolunteerAsync(Guid VolunteerId) // Change to Guid to match VolunteerId type
        {
            var volunteer = await _volunteerRepository.GetByIdAsync(VolunteerId);
            if (volunteer != null)
            {
                await _volunteerRepository.DeleteAsync(volunteer);
            }
        }
    }
}
