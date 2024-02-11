using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using H4H.Domain.Entities;
using H4H.Domain.Interfaces;
using H4H.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace H4H.Infrastructure.Repositories
{
    public class AddressRepository : IAddressRepository
    {
        private readonly H4HDbContext _context;

        public AddressRepository(H4HDbContext context)
        {
            _context = context;
        }

        public async Task<Address> GetByIdAsync(int id)
        {
            return await _context.Addresses.FindAsync(id);
        }

        public async Task<List<Address>> GetAllAsync()
        {
            return await _context.Addresses.ToListAsync();
        }

        public async Task AddAsync(Address entity)
        {
            await _context.Addresses.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Address entity)
        {
            _context.Addresses.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Address entity)
        {
            _context.Addresses.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}
