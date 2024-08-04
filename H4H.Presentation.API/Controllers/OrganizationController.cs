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
    public class OrganizationController : ControllerBase
    {
        private readonly IOrganizationService _organizationService;
        private readonly IMapper _mapper;

        public OrganizationController(IOrganizationService organizationService, IMapper mapper)
        {
            _organizationService = organizationService;
            _mapper = mapper;
        }

        // GET: api/organization
        [HttpGet]
        public async Task<IActionResult> GetAllOrganizations()
        {
            var organizations = await _organizationService.GetAllOrganizationsAsync();
            var organizationDtos = _mapper.Map<IEnumerable<OrganizationDto>>(organizations);
            return Ok(organizationDtos);
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
            var organizationDto = _mapper.Map<OrganizationDto>(organization);
            return Ok(organizationDto);
        }

        // POST: api/organization
        [HttpPost]
        public async Task<IActionResult> CreateOrganization([FromBody] OrganizationDto organizationDto)
        {
            if (organizationDto == null)
            {
                return BadRequest("Organization is null.");
            }

            var organization = _mapper.Map<Organization>(organizationDto);
            await _organizationService.AddOrganizationAsync(organization);
            var createdOrganizationDto = _mapper.Map<OrganizationDto>(organization);
            return CreatedAtAction(nameof(GetOrganizationById), new { OrganizationId = organization.OrganizationId }, createdOrganizationDto);
        }

        // PUT: api/organization/{OrganizationId}
        [HttpPut("{OrganizationId}")]
        public async Task<IActionResult> UpdateOrganization(Guid OrganizationId, [FromBody] OrganizationDto organizationDto)
        {
            if (organizationDto == null)
            {
                return BadRequest("Organization is null.");
            }

            var existingOrganization = await _organizationService.GetOrganizationByIdAsync(OrganizationId);
            if (existingOrganization == null)
            {
                return NotFound($"Organization with ID {OrganizationId} not found.");
            }

            _mapper.Map(organizationDto, existingOrganization);
            await _organizationService.UpdateOrganizationAsync(existingOrganization);
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