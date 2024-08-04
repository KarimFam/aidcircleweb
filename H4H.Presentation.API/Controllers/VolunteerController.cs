using Microsoft.AspNetCore.Mvc;
using H4H.Application.Interfaces;
using H4H.Domain.Entities;
using H4H.Application.DTOs;
using System;
using System.Threading.Tasks;
using AutoMapper;

namespace H4H.Presentation.API.Controllers
    {
        [ApiController]
        [Route("api/[controller]")]
        public class VolunteerController : ControllerBase
        {
            private readonly IVolunteerService _volunteerService;
            private readonly IMapper _mapper;

            public VolunteerController(IVolunteerService volunteerService, IMapper mapper)
            {
                _volunteerService = volunteerService;
                _mapper = mapper;
            }

            // GET: api/volunteer
            [HttpGet]
            public async Task<IActionResult> GetAllVolunteers()
            {
                var volunteers = await _volunteerService.GetAllVolunteersAsync();
                var volunteerDtos = _mapper.Map<IEnumerable<VolunteerDto>>(volunteers);
                return Ok(volunteerDtos);
            }

            // GET: api/volunteer/{VolunteerId}
            [HttpGet("{VolunteerId}")]
            public async Task<IActionResult> GetVolunteerById(Guid VolunteerId)
            {
                var volunteer = await _volunteerService.GetVolunteerByIdAsync(VolunteerId);
                if (volunteer == null)
                {
                    return NotFound($"Volunteer with ID {VolunteerId} not found.");
                }
                var volunteerDto = _mapper.Map<VolunteerDto>(volunteer);
                return Ok(volunteerDto);
            }

            // POST: api/volunteer
            [HttpPost]
            public async Task<IActionResult> CreateVolunteer([FromBody] VolunteerDto volunteerDto)
            {
                if (volunteerDto == null)
                {
                    return BadRequest("Volunteer is null.");
                }

                var volunteer = _mapper.Map<Volunteer>(volunteerDto);
                await _volunteerService.AddVolunteerAsync(volunteer);
                var createdVolunteerDto = _mapper.Map<VolunteerDto>(volunteer);
                return CreatedAtAction(nameof(GetVolunteerById), new { VolunteerId = volunteer.VolunteerId }, createdVolunteerDto);
            }

            // PUT: api/volunteer/{VolunteerId}
            [HttpPut("{VolunteerId}")]
            public async Task<IActionResult> UpdateVolunteer(Guid VolunteerId, [FromBody] VolunteerDto volunteerDto)
            {
                if (volunteerDto == null)
                {
                    return BadRequest("Volunteer is null.");
                }

                var existingVolunteer = await _volunteerService.GetVolunteerByIdAsync(VolunteerId);
                if (existingVolunteer == null)
                {
                    return NotFound($"Volunteer with ID {VolunteerId} not found.");
                }

                _mapper.Map(volunteerDto, existingVolunteer);
                await _volunteerService.UpdateVolunteerAsync(existingVolunteer);
                return NoContent();
            }

        [HttpDelete("{VolunteerId}")]
        public async Task<IActionResult> DeleteVolunteer(Guid VolunteerId)
        {
            var volunteer = await _volunteerService.GetVolunteerByIdAsync(VolunteerId);
            if (volunteer == null)
            {
                return NotFound($"Volunteer with ID {VolunteerId} not found.");
            }

            await _volunteerService.DeleteVolunteerAsync(VolunteerId);
            return NoContent();
        }
    }
        
    }
