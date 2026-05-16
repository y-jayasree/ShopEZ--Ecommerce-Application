using Moq;
using ShopEZ_OrderService.Models;
using ShopEZ_OrderService.Repositories.Interfaces;
using ShopEZ_OrderService.Services;
using ShopEZ_Shared_Lib.DTOs;
using Xunit;

namespace ShopEZ_OrderService.Tests.Services
{
    public class OrderServiceTests
    {
        // Helpers / shared setup

        private readonly Mock<IOrderRepository> _repoMock = new();
        private OrderService CreateService() => new(_repoMock.Object);

        private static CreateOrderDTO ValidOrderDto(int itemCount = 1) => new()
        {
            ShippingAddress = "123 Main St",
            PaymentMethod = "CASH_ON_DELIVERY",
            Items = Enumerable.Range(1, itemCount).Select(i => new OrderItemDTO
            {
                ProductId = i,
                ProductName = $"Product {i}",
                Quantity = 2,
                UnitPrice = 10.00m * i
            }).ToList()
        };

        private static Order OrderEntity(int orderId, int userId, List<OrderItemDTO> items) => new()
        {
            OrderId = orderId,
            UserId = userId,
            OrderNumber = $"ORD-20260512-ABC{orderId:D3}",
            OrderDate = DateTime.UtcNow,
            TotalAmount = items.Sum(i => i.UnitPrice * i.Quantity),
            Status = "PENDING",
            ShippingAddress = "123 Main St",
            PaymentMethod = "CASH_ON_DELIVERY",
            Items = items.Select(i => new OrderItem
            {
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice
            }).ToList()
        };

        // PlaceOrderAsync

        [Fact]
        public async Task PlaceOrder_ValidDto_CreatesOrderAndReturnsResponse()
        {
            var dto = ValidOrderDto(2);
            var entity = OrderEntity(1, 42, dto.Items);

            _repoMock.Setup(r => r.CreateAsync(It.IsAny<Order>()))
                     .ReturnsAsync(entity);

            var result = await CreateService().PlaceOrderAsync(42, dto);

            Assert.NotNull(result);
            Assert.Equal(1, result.OrderId);
            Assert.Equal(42, result.UserId);
            Assert.Equal("PENDING", result.Status);
            Assert.False(string.IsNullOrWhiteSpace(result.OrderNumber));
            Assert.Equal(2, result.Items.Count);
        }

        [Fact]
        public async Task PlaceOrder_TotalAmountCalculatedCorrectly()
        { 
            var dto = ValidOrderDto(2);
            var entity = OrderEntity(1, 1, dto.Items);

            _repoMock.Setup(r => r.CreateAsync(It.IsAny<Order>()))
                     .ReturnsAsync(entity);

            var result = await CreateService().PlaceOrderAsync(1, dto);

            Assert.Equal(60.00m, result.TotalAmount);
        }

        [Fact]
        public async Task PlaceOrder_EmptyItems_ThrowsArgumentException()
        {
            var dto = new CreateOrderDTO { Items = new List<OrderItemDTO>() };

            var ex = await Assert.ThrowsAsync<ArgumentException>(
                () => CreateService().PlaceOrderAsync(1, dto));

            Assert.Contains("at least one item", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task PlaceOrder_InitialStatusIsPending()
        {
            var dto = ValidOrderDto();
            var entity = OrderEntity(1, 1, dto.Items);

            _repoMock.Setup(r => r.CreateAsync(It.IsAny<Order>()))
                     .ReturnsAsync(entity);

            var result = await CreateService().PlaceOrderAsync(1, dto);

            Assert.Equal("PENDING", result.Status);
        }

        [Fact]
        public async Task PlaceOrder_OrderNumberFormatStartsWithORD()
        {
            var dto = ValidOrderDto();
            var entity = OrderEntity(99, 1, dto.Items);

            _repoMock.Setup(r => r.CreateAsync(It.IsAny<Order>()))
                     .ReturnsAsync(entity);

            var result = await CreateService().PlaceOrderAsync(1, dto);

            Assert.StartsWith("ORD-", result.OrderNumber);
        }

        // GetMyOrdersAsync

        [Fact]
        public async Task GetMyOrders_ReturnsAllOrdersForUser()
        {
            var orders = new List<Order>
            {
                OrderEntity(1, 7, ValidOrderDto(1).Items),
                OrderEntity(2, 7, ValidOrderDto(2).Items)
            };
            _repoMock.Setup(r => r.GetByUserIdAsync(7)).ReturnsAsync(orders);

            var result = await CreateService().GetMyOrdersAsync(7);

            Assert.Equal(2, result.Count);
            Assert.All(result, o => Assert.Equal(7, o.UserId));
        }

        [Fact]
        public async Task GetMyOrders_UserWithNoOrders_ReturnsEmptyList()
        {
            _repoMock.Setup(r => r.GetByUserIdAsync(99)).ReturnsAsync(new List<Order>());

            var result = await CreateService().GetMyOrdersAsync(99);

            Assert.Empty(result);
        }

        // GetByIdAsync
        [Fact]
        public async Task GetById_ExistingOrder_ReturnsMappedResponse()
        {
            var order = OrderEntity(5, 3, ValidOrderDto(1).Items);
            _repoMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(order);

            var result = await CreateService().GetByIdAsync(5);

            Assert.NotNull(result);
            Assert.Equal(5, result!.OrderId);
        }

        [Fact]
        public async Task GetById_NonExistingOrder_ReturnsNull()
        {
            _repoMock.Setup(r => r.GetByIdAsync(404)).ReturnsAsync((Order?)null);

            var result = await CreateService().GetByIdAsync(404);

            Assert.Null(result);
        }

        // UpdateStatusAsync

        [Fact]
        public async Task UpdateStatus_ExistingOrder_ChangesStatusAndReturns()
        {
            var order = OrderEntity(10, 1, ValidOrderDto(1).Items);

            _repoMock.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(order);
            _repoMock.Setup(r => r.UpdateAsync(It.IsAny<Order>()))
                     .ReturnsAsync((Order o) => o);

            var result = await CreateService().UpdateStatusAsync(10, "SHIPPED");

            Assert.Equal("SHIPPED", result.Status);
            _repoMock.Verify(r => r.UpdateAsync(It.Is<Order>(o =>
                o.Status == "SHIPPED")), Times.Once);
        }

        [Fact]
        public async Task UpdateStatus_NonExistingOrder_ThrowsKeyNotFoundException()
        {
            _repoMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Order?)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => CreateService().UpdateStatusAsync(999, "CANCELLED"));
        }


        // GetAllOrdersAsync (pagination)
        [Fact]
        public async Task GetAllOrders_ReturnsPagedResultWithTotal()
        {
            var orders = new List<Order>
            {
                OrderEntity(1, 1, ValidOrderDto(1).Items),
                OrderEntity(2, 2, ValidOrderDto(1).Items)
            };
            _repoMock.Setup(r => r.GetAllAsync(1, 10)).ReturnsAsync((orders, 2));

            var (result, total) = await CreateService().GetAllOrdersAsync(1, 10);

            Assert.Equal(2, result.Count);
            Assert.Equal(2, total);
        }
    }
}