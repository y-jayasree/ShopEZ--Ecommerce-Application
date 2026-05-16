using ShopEZ_OrderService.Models;
using ShopEZ_OrderService.Repositories.Interfaces;
using ShopEZ_Shared_Lib.DTOs;

namespace ShopEZ_OrderService.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orders;
        public OrderService(IOrderRepository orders)
        {
            _orders = orders;
        }

        public async Task<OrderResponseDTO> PlaceOrderAsync(int userId, CreateOrderDTO dto)
        {
            if (!dto.Items.Any())
                throw new ArgumentException("Order must have at least one item");

            var totalAmount = dto.Items.Sum(i => i.UnitPrice * i.Quantity);
            var order = new Order
            {
                UserId = userId,
                OrderNumber = GenerateOrderNumber(),
                OrderDate = DateTime.UtcNow,
                TotalAmount = totalAmount,
                Status = "PENDING",
                ShippingAddress = dto.ShippingAddress,
                PaymentMethod = dto.PaymentMethod,
                Items = dto.Items.Select(i => new OrderItem
                {
                    ProductId = i.ProductId,
                    ProductName = i.ProductName,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    ImageUrl = i.ImageUrl 
                }).ToList()
            };

            var created = await _orders.CreateAsync(order);
            return MapToResponse(created);
        }

        public async Task<List<OrderResponseDTO>> GetMyOrdersAsync(int userId)
        {
            var orders = await _orders.GetByUserIdAsync(userId);
            return orders.Select(MapToResponse).ToList();
        }

        public async Task<OrderResponseDTO?> GetByIdAsync(int orderId)
        {
            var order = await _orders.GetByIdAsync(orderId);
            return order == null ? null : MapToResponse(order);
        }

        public async Task<(List<OrderResponseDTO> Orders, int Total)> GetAllOrdersAsync(int page, int pageSize)
        {
            var (orders, total) = await _orders.GetAllAsync(page, pageSize);
            return (orders.Select(MapToResponse).ToList(), total);
        }

        public async Task<OrderResponseDTO> UpdateStatusAsync(int orderId, string status)
        {
            var order = await _orders.GetByIdAsync(orderId)
                ?? throw new KeyNotFoundException("Order not found");
            order.Status = status;
            await _orders.UpdateAsync(order);
            return MapToResponse(order);
        }

        private static OrderResponseDTO MapToResponse(Order order) => new()
        {
            OrderId = order.OrderId,
            UserId = order.UserId,
            OrderNumber = order.OrderNumber,
            OrderDate = order.OrderDate,
            TotalAmount = order.TotalAmount,
            Status = order.Status,
            ShippingAddress = order.ShippingAddress,
            PaymentMethod = order.PaymentMethod,
            Items = order.Items.Select(i => new OrderItemResponseDTO
            {
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                ImageUrl = i.ImageUrl  
            }).ToList()
        };

        private static string GenerateOrderNumber()
            => $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6].ToUpper()}";
    }
}