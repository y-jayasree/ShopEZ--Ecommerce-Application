using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using ShopEZ_OrderService.Controllers;
using ShopEZ_OrderService.Services;
using ShopEZ_Shared_Lib.DTOs;
using System.Security.Claims;
using Xunit;

namespace ShopEZ_OrderService.Tests.Controllers
{
    public class OrdersControllerTests
    {
        private readonly Mock<IOrderService> _serviceMock = new();

        private OrdersController CreateController(int userId, string role = "CUSTOMER")
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Role, role)
            };
            var identity = new ClaimsIdentity(claims, "Test");
            var principal = new ClaimsPrincipal(identity);

            return new OrdersController(_serviceMock.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { User = principal }
                }
            };
        }

        private static OrderResponseDTO SampleOrder(int orderId, int userId) => new()
        {
            OrderId = orderId,
            UserId = userId,
            OrderNumber = "ORD-20260512-AABBCC",
            Status = "PENDING",
            TotalAmount = 50.00m,
            Items = new List<OrderItemResponseDTO>
            {
                new() { ProductId = 1, ProductName = "Pen", Quantity = 2, UnitPrice = 25.00m }
            }
        };

        //  POST /api/orders 

        [Fact]
        public async Task PlaceOrder_ValidDto_Returns201()
        {
            var dto = new CreateOrderDTO
            {
                Items = new List<OrderItemDTO> { new() { ProductId = 1, ProductName = "Pen", Quantity = 2, UnitPrice = 25m } },
                ShippingAddress = "1 Main St",
                PaymentMethod = "CASH_ON_DELIVERY"
            };
            var order = SampleOrder(1, 1);

            _serviceMock.Setup(s => s.PlaceOrderAsync(1, dto)).ReturnsAsync(order);

            var result = await CreateController(1).PlaceOrder(dto) as ObjectResult;

            Assert.Equal(201, result?.StatusCode);
        }

        [Fact]
        public async Task PlaceOrder_EmptyItems_ServiceThrows_BubblesUp()
        {
            var dto = new CreateOrderDTO { Items = new List<OrderItemDTO>() };

            _serviceMock.Setup(s => s.PlaceOrderAsync(1, dto))
                        .ThrowsAsync(new ArgumentException("Order must have at least one item"));

            await Assert.ThrowsAsync<ArgumentException>(
                () => CreateController(1).PlaceOrder(dto));
        }

        //  GET /api/orders/my-orders 

        [Fact]
        public async Task GetMyOrders_Returns200WithList()
        {
            var orders = new List<OrderResponseDTO> { SampleOrder(1, 1), SampleOrder(2, 1) };
            _serviceMock.Setup(s => s.GetMyOrdersAsync(1)).ReturnsAsync(orders);

            var result = await CreateController(1).GetMyOrders() as OkObjectResult;

            Assert.Equal(200, result?.StatusCode);
        }

        //  GET /api/orders/{id} 

        [Fact]
        public async Task GetById_OwnerUser_Returns200()
        {
            _serviceMock.Setup(s => s.GetByIdAsync(10)).ReturnsAsync(SampleOrder(10, 1));

            var result = await CreateController(userId: 1).GetById(10) as OkObjectResult;

            Assert.Equal(200, result?.StatusCode);
        }

        [Fact]
        public async Task GetById_NonExistingOrder_Returns404()
        {
            _serviceMock.Setup(s => s.GetByIdAsync(999)).ReturnsAsync((OrderResponseDTO?)null);

            var result = await CreateController(1).GetById(999);

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task GetById_OtherUsersOrder_ReturnsForbid()
        {
            _serviceMock.Setup(s => s.GetByIdAsync(5)).ReturnsAsync(SampleOrder(5, userId: 2));

            var result = await CreateController(userId: 1, role: "CUSTOMER").GetById(5);

            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task GetById_AdminCanViewAnyOrder_Returns200()
        {
            _serviceMock.Setup(s => s.GetByIdAsync(5)).ReturnsAsync(SampleOrder(5, userId: 2));

            var result = await CreateController(userId: 99, role: "ADMIN").GetById(5) as OkObjectResult;

            Assert.Equal(200, result?.StatusCode);
        }

        //  PATCH /api/orders/{id}/status 

        [Fact]
        public async Task UpdateStatus_ValidStatus_Returns200()
        {
            var dto = new UpdateOrderStatusDTO { Status = "SHIPPED" };
            var updated = SampleOrder(1, 1);
            updated.Status = "SHIPPED";

            _serviceMock.Setup(s => s.UpdateStatusAsync(1, "SHIPPED")).ReturnsAsync(updated);

            var result = await CreateController(1, "ADMIN").UpdateStatus(1, dto) as OkObjectResult;

            Assert.Equal(200, result?.StatusCode);
        }

        [Fact]
        public async Task UpdateStatus_NonExistingOrder_ServiceThrows_BubblesUp()
        {
            _serviceMock.Setup(s => s.UpdateStatusAsync(999, "CANCELLED"))
                        .ThrowsAsync(new KeyNotFoundException("Order not found"));

            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => CreateController(1, "ADMIN").UpdateStatus(999, new UpdateOrderStatusDTO { Status = "CANCELLED" }));
        }
    }
}