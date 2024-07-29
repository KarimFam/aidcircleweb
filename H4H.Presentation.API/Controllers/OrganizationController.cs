using Microsoft.AspNetCore.Mvc;
using H4H.Application.Interfaces;
using H4H.Domain.Entities;
using System.Threading.Tasks;

namespace H4H.Presentation.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrganizationController : ControllerBase
    {
        private readonly IOrganizationService _organizationService;

        public OrganizationController(IOrganizationService organizationService)
        {
            _organizationService = organizationService;
        }

        // GET: api/organization
        [HttpGet]
        public async Task<IActionResult> GetAllOrganizations()
        {
            var organizations = await _organizationService.GetAllOrganizationsAsync();
            return Ok(organizations);
        }

        // GET: api/organization/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrganizationById(Guid id)
        {
            var organization = await _organizationService.GetOrganizationByIdAsync(id);
            if (organization == null)
            {
                return NotFound($"Organization with ID {id} not found.");
            }
            return Ok(organization);
        }

        // POST: api/organization
        [HttpPost]
        public async Task<IActionResult> CreateOrganization([FromBody] Organization organization)
        {
            if (organization == null)
            {
                return BadRequest("Organization data is null.");
            }

            await _organizationService.AddOrganizationAsync(organization);
            return CreatedAtAction(nameof(GetOrganizationById), new { id = organization.OrganizationId }, organization);
        }

        // PUT: api/organization/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateOrganization(Guid id, [FromBody] Organization organization)
        {
            if (organization == null || id != organization.OrganizationId)
            {
                return BadRequest("Organization data is null or ID mismatch.");
            }

            await _organizationService.UpdateOrganizationAsync(organization);
            return NoContent();
        }

        // DELETE: api/organization/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrganization(Guid id)
        {
            var organization = await _organizationService.GetOrganizationByIdAsync(id);
            if (organization == null)
            {
                return NotFound($"Organization with ID {id} not found.");
            }

            await _organizationService.DeleteOrganizationAsync(id);
            return NoContent();
        }
    }
}
