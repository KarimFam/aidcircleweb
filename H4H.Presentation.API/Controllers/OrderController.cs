using Microsoft.AspNetCore.Mvc;
using H4H.Application.Interfaces;
using H4H.Domain.Entities;
using H4H.Presentation.API.Data;
using System;
using System.Threading.Tasks;
using AutoMapper;
namespace H4H.Presentation.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly IMapper _mapper;

        public OrderController(IOrderService orderService, IMapper mapper)
        {
            _orderService = orderService;
            _mapper = mapper;
        }

        // GET: api/order
        [HttpGet]
        public async Task<IActionResult> GetAllOrders()
        {
            var orders = await _orderService.GetAllOrdersAsync();
            var orderDtos = _mapper.Map<IEnumerable<OrderDto>>(orders);
            return Ok(orderDtos);
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
            var orderDto = _mapper.Map<OrderDto>(order);
            return Ok(orderDto);
        }

        // POST: api/order
        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] OrderDto orderDto)
        {
            if (orderDto == null)
            {
                return BadRequest("Order is null.");
            }

            var order = _mapper.Map<Order>(orderDto);
            await _orderService.AddOrderAsync(order);
            var createdOrderDto = _mapper.Map<OrderDto>(order);
            return CreatedAtAction(nameof(GetOrderById), new { OrderId = order.OrderId }, createdOrderDto);
        }

        // PUT: api/order/{OrderId}
        [HttpPut("{OrderId}")]
        public async Task<IActionResult> UpdateOrder(Guid OrderId, [FromBody] OrderDto orderDto)
        {
            if (orderDto == null)
            {
                return BadRequest("Order is null.");
            }

            var existingOrder = await _orderService.GetOrderByIdAsync(OrderId);
            if (existingOrder == null)
            {
                return NotFound($"Order with ID {OrderId} not found.");
            }

            _mapper.Map(orderDto, existingOrder);
            await _orderService.UpdateOrderAsync(existingOrder);
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