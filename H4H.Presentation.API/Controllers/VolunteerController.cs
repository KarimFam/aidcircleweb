﻿using Microsoft.AspNetCore.Mvc;
using H4H.Application.Interfaces;
using H4H.Domain.Entities;
using System.Threading.Tasks;

namespace H4H.Presentation.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VolunteerController : ControllerBase
    {
        private readonly IVolunteerService _volunteerService;

        public VolunteerController(IVolunteerService volunteerService)
        {
            _volunteerService = volunteerService;
        }

        // GET: api/volunteer
        [HttpGet]
        public async Task<IActionResult> GetAllVolunteers()
        {
            var volunteers = await _volunteerService.GetAllVolunteersAsync();
            return Ok(volunteers);
        }

        // GET: api/volunteer/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetVolunteerById(Guid volunteerId)
        {
            var volunteer = await _volunteerService.GetVolunteerByIdAsync(volunteerId);
            if (volunteer == null)
            {
                return NotFound($"Volunteer with ID {volunteerId} not found.");
            }
            return Ok(volunteer);
        }

        // POST: api/volunteer
        [HttpPost]
        public async Task<IActionResult> CreateVolunteer([FromBody] Volunteer volunteer)
        {
            if (volunteer == null)
            {
                return BadRequest("Volunteer data is null.");
            }

            await _volunteerService.AddVolunteerAsync(volunteer);
            return CreatedAtAction(nameof(GetVolunteerById), new { volunteerId = volunteer.VolunteerId }, volunteer);
        }

        // PUT: api/volunteer/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateVolunteer(Guid id, [FromBody] Volunteer volunteer)
        {
            if (volunteer == null || id != volunteer.VolunteerId)
            {
                return BadRequest("Volunteer data is null or ID mismatch.");
            }

            await _volunteerService.UpdateVolunteerAsync(volunteer);
            return NoContent();
        }

        // DELETE: api/volunteer/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVolunteer(Guid volunteerId)
        {
            var volunteer = await _volunteerService.GetVolunteerByIdAsync(volunteerId);
            if (volunteer == null)
            {
                return NotFound($"Volunteer with ID {volunteerId} not found.");
            }

            await _volunteerService.DeleteVolunteerAsync(volunteerId);
            return NoContent();
        }
    }
}
