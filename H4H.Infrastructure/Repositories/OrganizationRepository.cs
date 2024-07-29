using H4H.Domain.Entities;
using H4H.Domain.Interfaces;
using H4H.Infrastructure.Data;
using H4H.Infrastructure.Data.Contexts;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace H4H.Infrastructure.Repositories
{
    public class OrganizationRepository : IOrganizationRepository
    {
        private readonly H4HDbContext _context;

        public OrganizationRepository(H4HDbContext context)
        {
            _context = context;
        }

        public async Task<Organization> GetByIdAsync(int id)
        {
            return await _context.Organizations
                                 .Include(org => org.Addresses) // Include related entities as needed
                                 .FirstOrDefaultAsync(org => org.OrganizationId == id);
        }

        public async Task<List<Organization>> GetAllAsync()
        {
            return await _context.Organizations
                                 .Include(org => org.Addresses) // Include related entities as needed
                                 .ToListAsync();
        }

        public async Task AddAsync(Organization organization)
        {
            await _context.Organizations.AddAsync(organization);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Organization organization)
        {
            _context.Organizations.Update(organization);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Organization organization)
        {
            _context.Organizations.Remove(organization);
            await _context.SaveChangesAsync();
        }
    }
}
