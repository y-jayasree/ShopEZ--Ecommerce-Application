using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopEZ_OrderService.Services;
using ShopEZ_Shared_Lib.DTOs;
using ShopEZ_Shared_Lib.Helpers;

namespace ShopEZ_OrderService.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/orders")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpPost]
        public async Task<IActionResult> PlaceOrder([FromBody] CreateOrderDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Fail(GetModelErrors()));

            var userId = JwtHelper.GetUserId(User);
            if (userId == 0)
                return Unauthorized(ApiResponse<object>.Fail("Invalid token"));

            var result = await _orderService.PlaceOrderAsync(userId, dto);
            return StatusCode(201, ApiResponse<OrderResponseDTO>.Ok(result, "Order placed successfully"));
        }

        [HttpGet("my-orders")]
        public async Task<IActionResult> GetMyOrders()
        {
            var userId = JwtHelper.GetUserId(User);
            if (userId == 0)
                return Unauthorized(ApiResponse<object>.Fail("Invalid token"));

            var orders = await _orderService.GetMyOrdersAsync(userId);
            return Ok(ApiResponse<List<OrderResponseDTO>>.Ok(orders, "Orders retrieved", orders.Count));
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var userId = JwtHelper.GetUserId(User);
            var role = JwtHelper.GetRole(User);

            var order = await _orderService.GetByIdAsync(id);
            if (order == null)
                return NotFound(ApiResponse<object>.Fail("Order not found"));

            // Customers can only see their own orders
            if (role.ToUpper() != "ADMIN" && order.UserId != userId)
                return Forbid();

            return Ok(ApiResponse<OrderResponseDTO>.Ok(order));
        }

        [Authorize(Roles = "ADMIN")]
        [HttpGet("all-orders")]
        public async Task<IActionResult> GetAllOrders([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var (orders, total) = await _orderService.GetAllOrdersAsync(page, pageSize);
            return Ok(ApiResponse<List<OrderResponseDTO>>.Ok(orders, "All orders", total));
        }

        [Authorize(Roles = "ADMIN")]
        [HttpPatch("{id:int}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateOrderStatusDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Fail(GetModelErrors()));

            var updated = await _orderService.UpdateStatusAsync(id, dto.Status);
            return Ok(ApiResponse<OrderResponseDTO>.Ok(updated, "Order status updated"));
        }

        private string GetModelErrors()
        {
            return string.Join("; ", ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage));
        }
    }
}