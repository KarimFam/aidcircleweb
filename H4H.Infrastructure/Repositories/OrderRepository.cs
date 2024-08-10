using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using H4H.Domain.Entities;
using H4H.Infrastructure.Data.Contexts;
using Microsoft.EntityFrameworkCore;

public class OrderRepository : IOrderRepository
{
    private readonly H4HDbContext _context;

    public OrderRepository(H4HDbContext context)
    {
        _context = context;
    }

    public async Task<Order> GetOrderByIdAsync(Guid OrderId)
    {
        return await _context.Orders.FirstOrDefaultAsync(o => o.OrderId == OrderId);
    }

    public async Task<List<Order>> GetAllOrdersAsync()
    {
        return await _context.Orders.ToListAsync();
    }

    public async Task AddOrderAsync(Order order)
    {
        await _context.Orders.AddAsync(order);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateOrderAsync(Order order)
    {
        _context.Orders.Update(order);
        await _context.SaveChangesAsync();
    }

    public async Task<List<Order>> GetLatestOrdersAsync(DateTime fromDate)
    {
        return await _context.Orders
                             .Where(order => order.CreatedDate >= fromDate)
                             .ToListAsync();
    }

    public async Task DeleteOrderAsync(Order order)
    {
        _context.Orders.Remove(order);
        await _context.SaveChangesAsync();
    }
}