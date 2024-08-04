using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using H4H.Domain.Entities;
using H4H.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using H4H.Infrastructure.Data.Contexts;

namespace H4H.Infrastructure.Repositories
{
    public class VolunteerRepository : IVolunteerRepository
    {
        private readonly H4HDbContext _context;

        public VolunteerRepository(H4HDbContext context)
        {
            _context = context;
        }

        public async Task<Volunteer> GetByIdAsync(Guid VolunteerId)
        {
            return await _context.Volunteers.FindAsync(VolunteerId);
        }

        public async Task<List<Volunteer>> GetAllAsync()
        {
            return await _context.Volunteers.ToListAsync();
        }

        public async Task AddAsync(Volunteer entity)
        {
            await _context.Volunteers.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Volunteer entity)
        {
            _context.Volunteers.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Volunteer entity)
        {
            _context.Volunteers.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}
