﻿using System;
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
    public class AddressRepository : IAddressRepository
    {
        private readonly H4HDbContext _context;

        public AddressRepository(H4HDbContext context)
        {
            _context = context;
        }

        public async Task<Address> GetByIdAsync(Guid AddressId)
        {
            return await _context.Addresses.FindAsync(AddressId);
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

        // Add missing methods here

        //public async Task<List<Address>> GetByCityAsync(string city)
        //{
        //    return await _context.Addresses.Where(a => a.City == city).ToListAsync();
        //}

        //public async Task<List<Address>> GetByStateAsync(string state)
        //{
        //    return await _context.Addresses.Where(a => a.State == state).ToListAsync();
        //}

        //public async Task<List<Address>> GetByCountryAsync(string country)
        //{
        //    return await _context.Addresses.Where(a => a.Country == country).ToListAsync();
        //}

        //public async Task<List<Address>> GetByPostalCodeAsync(string postalCode)
        //{
        //    return await _context.Addresses.Where(a => a.PostalCode == postalCode).ToListAsync();
        //}

        public async Task<List<Address>> GetAddressByUserIdAsync(Guid UserId)
        {
            return await _context.Addresses.Where(a => a.AddressId == UserId).ToListAsync();
        }



        public async Task<List<Address>> GetAddressesByItemIdAsync(Guid ItemId)
        {
            return await _context.Addresses.Where(a => a.AddressId == ItemId).ToListAsync();
        }

        public async Task<List<Address>> GetAddressByOrderIdAsync(Guid OrderId)
        {
            return await _context.Addresses.Where(a => a.AddressId == OrderId).ToListAsync();
        }

        public async Task<List<Address>> GetAddressByOrganizationIdAsync(Guid OrganizationId)
        {
            return await _context.Addresses.Where(a => a.AddressId == OrganizationId).ToListAsync();
        }
        //public async Task<List<Address>> GetAddressesByOrganizationIdAsync(int organizationId)
        //{
        //    return await _context.Addresses.Where(a => a.AddressableId == organizationId && a.AddressableType == "Organization").ToListAsync();
        //}

        //public async Task<List<Address>> GetAddressesByItemIdAsync(int itemId)
        //{
        //    return await _context.Addresses.Where(a => a.AddressableId == itemId && a.AddressableType == "Item").ToListAsync();
        //}

    }
}
