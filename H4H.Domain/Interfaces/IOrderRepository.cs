using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using H4H.Domain.Entities;

public interface IOrderRepository
{
    Task<Order> GetOrderByIdAsync(Guid id);
    Task<List<Order>> GetAllOrdersAsync();
    Task AddOrderAsync(Order order);
    Task UpdateOrderAsync(Order order);
    Task DeleteOrderAsync(Order order);
}
