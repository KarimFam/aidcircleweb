using Microsoft.AspNetCore.Mvc;
using H4H.Application.Interfaces;
using H4H.Domain.Entities;
using System;
using System.Threading.Tasks;

namespace H4H.Presentation.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        // GET: api/order
        [HttpGet]
        public async Task<IActionResult> GetAllOrders()
        {
            var orders = await _orderService.GetAllOrdersAsync();
            return Ok(orders);
        }

        // GET: api/order/{OrderId}
        [HttpGet("{OrderId}")]
        public async Task<IActionResult> GetOrderById(Guid OrderId)
        {
            var order = await _orderService.GetOrderByIdAsync(OrderId);
            if (order == null)
            {
                return NotFound($"Order with ID {OrderId} not found.");
            }
            return Ok(order);
        }

        // POST: api/order
        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] Order order)
        {
            if (order == null)
            {
                return BadRequest("Order data is null.");
            }

            await _orderService.AddOrderAsync(order);
            return CreatedAtAction(nameof(GetOrderById), new { OrderId = order.OrderId }, order);
        }

        // PUT: api/order/{OrderId}
        [HttpPut("{OrderId}")]
        public async Task<IActionResult> UpdateOrder(Guid OrderId, [FromBody] Order order)
        {
            if (order == null || OrderId != order.OrderId)
            {
                return BadRequest("Order data is null or ID mismatch.");
            }

            await _orderService.UpdateOrderAsync(order);
            return NoContent();
        }

        // DELETE: api/order/{OrderId}
        [HttpDelete("{OrderId}")]
        public async Task<IActionResult> DeleteOrder(Guid OrderId)
        {
            var order = await _orderService.GetOrderByIdAsync(OrderId);
            if (order == null)
            {
                return NotFound($"Order with ID {OrderId} not found.");
            }

            await _orderService.DeleteOrderAsync(OrderId);
            return NoContent();
        }
    }
}