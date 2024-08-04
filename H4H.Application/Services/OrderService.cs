using H4H.Application.Interfaces;
using H4H.Domain.Entities;
using H4H.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace H4H.Application.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;

        public OrderService(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<IEnumerable<Order>> GetAllOrdersAsync()
        {
            return await _orderRepository.GetAllOrdersAsync();
        }

        public async Task<Order> GetOrderByIdAsync(Guid OrderId)
        {
            return await _orderRepository.GetOrderByIdAsync(OrderId);
        }

        public async Task AddOrderAsync(Order order)
        {
            await _orderRepository.AddOrderAsync(order);
        }

        public async Task UpdateOrderAsync(Order order)
        {
            await _orderRepository.UpdateOrderAsync(order);
        }

        public async Task DeleteOrderAsync(Guid OrderId)
        {
            var order = await _orderRepository.GetOrderByIdAsync(OrderId);
            if (order != null)
            {
                await _orderRepository.DeleteOrderAsync(order);
            }
        }
    }
}