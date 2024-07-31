using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using H4H.Domain.Entities;

public interface IOrderRepository
{
    Task<Order> GetByIdAsync(Guid id);
    Task<List<Order>> GetAllAsync();
    Task AddAsync(Order order);
    Task UpdateAsync(Order order);
    Task DeleteAsync(Order order);
}
