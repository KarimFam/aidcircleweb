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

        public OrganizationService(IOrganizationRepository organizationRepository)
        {
            _organizationRepository = organizationRepository;
        }

        public async Task<IEnumerable<Organization>> GetAllOrganizationsAsync()
        {
            return await _organizationRepository.GetAllAsync();
        }

        public async Task<Organization> GetOrganizationByIdAsync(Guid organizationId)
        {
            return await _organizationRepository.GetByIdAsync(organizationId);
        }

        public async Task AddOrganizationAsync(Organization organization)
        {
            await _organizationRepository.AddAsync(organization);
        }

        public async Task UpdateOrganizationAsync(Organization organization)
        {
            await _organizationRepository.UpdateAsync(organization);
        }

        public async Task DeleteOrganizationAsync(Guid organizationId)
        {
            var organization = await _organizationRepository.GetByIdAsync(organizationId);
            if (organization != null)
            {
                await _organizationRepository.DeleteAsync(organization);
            }
        }
    }
}
