using H4H.Domain.Entities;
using H4H.Domain.Interfaces;
using H4H.Application.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace H4H.Application.Services
{
    public class OrganizationService : IOrganizationService
    {
        private readonly IOrganizationRepository _organizationRepository;
        private readonly ILogger<OrganizationService> _logger;

        public OrganizationService(IOrganizationRepository organizationRepository, ILogger<OrganizationService> logger)
        {
            _organizationRepository = organizationRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<Organization>> GetAllOrganizationsAsync()
        {
            try
            {
                return await _organizationRepository.GetAllAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving organizations");
                throw;
            }
        }

        public async Task<Organization> GetOrganizationByIdAsync(Guid id)
        {
            try
            {
                return await _organizationRepository.GetByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving organization with ID {id}");
                throw;
            }
        }

        public async Task AddOrganizationAsync(Organization organization)
        {
            try
            {
                await _organizationRepository.AddAsync(organization);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding new organization");
                throw;
            }
        }

        public async Task UpdateOrganizationAsync(Organization organization)
        {
            try
            {
                await _organizationRepository.UpdateAsync(organization);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating organization with ID {organization.OrganizationId}");
                throw;
            }
        }

        public async Task DeleteOrganizationAsync(Guid id)
        {
            try
            {
                var organization = await _organizationRepository.GetByIdAsync(id);
                if (organization != null)
                {
                    await _organizationRepository.DeleteAsync(organization);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting organization with ID {id}");
                throw;
            }
        }
    }
}
