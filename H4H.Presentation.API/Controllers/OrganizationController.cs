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

        // GET: api/organization/{OrganizationId}
        [HttpGet("{OrganizationId}")]
        public async Task<IActionResult> GetOrganizationById(Guid OrganizationId)
        {
            var organization = await _organizationService.GetOrganizationByIdAsync(OrganizationId);
            if (organization == null)
            {
                return NotFound($"Organization with ID {OrganizationId} not found.");
            }
            return Ok(organization);
        }

        // POST: api/organization
        [HttpPost]
        public async Task<IActionResult> CreateOrganization([FromBody] Organization organization)
        {
            if (organization == null)
            {
                return BadRequest("Organization is null.");
            }

            await _organizationService.AddOrganizationAsync(organization);
            return CreatedAtAction(nameof(GetOrganizationById), new { OrganizationId = organization.OrganizationId }, organization);
        }

        // PUT: api/organization/{OrganizationId}
        [HttpPut("{OrganizationId}")]
        public async Task<IActionResult> UpdateOrganization(Guid OrganizationId, [FromBody] Organization organization)
        {
            if (organization == null || OrganizationId != organization.OrganizationId)
            {
                return BadRequest("Organization is null or ID mismatch.");
            }

            await _organizationService.UpdateOrganizationAsync(organization);
            return NoContent();
        }

        // DELETE: api/organization/{OrganizationId}
        [HttpDelete("{OrganizationId}")]
        public async Task<IActionResult> DeleteOrganization(Guid OrganizationId)
        {
            var organization = await _organizationService.GetOrganizationByIdAsync(OrganizationId);
            if (organization == null)
            {
                return NotFound($"Organization with ID {OrganizationId} not found.");
            }

            await _organizationService.DeleteOrganizationAsync(OrganizationId);
            return NoContent();
        }
    }
}