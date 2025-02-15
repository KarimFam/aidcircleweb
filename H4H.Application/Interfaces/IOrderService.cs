﻿using H4H.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace H4H.Application.Interfaces
{
    public interface IOrderService
    {
        Task<IEnumerable<Order>> GetAllOrdersAsync();
        Task<List<Order>> GetLatestOrdersAsync();
        Task<Order> GetOrderByIdAsync(Guid OrderId);
        Task AddOrderAsync(Order order);
        Task UpdateOrderAsync(Order order);
        Task DeleteOrderAsync(Guid OrderId);
    }
}