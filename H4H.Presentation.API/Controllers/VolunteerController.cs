using Microsoft.AspNetCore.Mvc;
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
        public async Task<IActionResult> GetVolunteerById(int id)
        {
            var volunteer = await _volunteerService.GetVolunteerByIdAsync(id);
            if (volunteer == null)
            {
                return NotFound($"Volunteer with ID {id} not found.");
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
            return CreatedAtAction(nameof(GetVolunteerById), new { id = volunteer.Id }, volunteer);
        }

        // PUT: api/volunteer/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateVolunteer(int id, [FromBody] Volunteer volunteer)
        {
            if (volunteer == null || id != volunteer.Id)
            {
                return BadRequest("Volunteer data is null or ID mismatch.");
            }

            await _volunteerService.UpdateVolunteerAsync(volunteer);
            return NoContent();
        }

        // DELETE: api/volunteer/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVolunteer(int id)
        {
            var volunteer = await _volunteerService.GetVolunteerByIdAsync(id);
            if (volunteer == null)
            {
                return NotFound($"Volunteer with ID {id} not found.");
            }

            await _volunteerService.DeleteVolunteerAsync(id);
            return NoContent();
        }
    }
}
