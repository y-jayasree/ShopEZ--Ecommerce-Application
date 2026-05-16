using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using ShopEZ_cartservice.Controllers;
using ShopEZ_cartservice.Services.Interfaces;
using ShopEZ_Shared_Lib.DTOs;
using ShopEZ_Shared_Lib.Helpers;
using System.Security.Claims;
using Xunit;

namespace ShopEZ_CartService.Tests.Controllers
{
    public class CartControllerTests
    {
        private readonly Mock<ICartService> _serviceMock = new();

        /// Creates a controller with a fake authenticated user whose id = <paramref name="userId"/>.
        private CartController CreateController(int userId)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Role, "CUSTOMER")
            };
            var identity = new ClaimsIdentity(claims, "Test");
            var principal = new ClaimsPrincipal(identity);

            var controller = new CartController(_serviceMock.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { User = principal }
                }
            };
            return controller;
        }

        private static CartResponseDTO EmptyCart(int userId) =>
            new() { UserId = userId, Items = new List<CartItemResponseDTO>() };

        private static CartResponseDTO CartWith(int userId, params CartItemResponseDTO[] items) =>
            new() { UserId = userId, Items = items.ToList() };

        //  GET /api/cart 

        [Fact]
        public async Task GetCart_AuthenticatedUser_Returns200()
        {
            _serviceMock.Setup(s => s.GetCartAsync(1)).ReturnsAsync(EmptyCart(1));

            var result = await CreateController(1).GetCart() as OkObjectResult;

            Assert.Equal(200, result?.StatusCode);
        }

        //  POST /api/cart/items 

        [Fact]
        public async Task AddItem_ValidDto_Returns200WithCart()
        {
            var dto = new AddToCartDTO { ProductId = 5, ProductName = "Hat", UnitPrice = 19.99m, Quantity = 1 };
            var cart = CartWith(1, new CartItemResponseDTO { ProductId = 5, ProductName = "Hat", UnitPrice = 19.99m, Quantity = 1 });

            _serviceMock.Setup(s => s.AddItemAsync(1, dto)).ReturnsAsync(cart);

            var result = await CreateController(1).AddItem(dto) as OkObjectResult;

            Assert.Equal(200, result?.StatusCode);
        }

        //  PUT /api/cart/items/{productId} 

        [Fact]
        public async Task UpdateItem_ExistingItem_Returns200()
        {
            var dto = new UpdateCartItemDTO { Quantity = 3 };
            var cart = CartWith(1, new CartItemResponseDTO { ProductId = 5, Quantity = 3 });

            _serviceMock.Setup(s => s.UpdateItemAsync(1, 5, dto)).ReturnsAsync(cart);

            var result = await CreateController(1).UpdateItem(5, dto) as OkObjectResult;

            Assert.Equal(200, result?.StatusCode);
        }

        [Fact]
        public async Task UpdateItem_ItemNotInCart_ServiceThrows_BubblesUp()
        {
            var dto = new UpdateCartItemDTO { Quantity = 1 };
            _serviceMock.Setup(s => s.UpdateItemAsync(1, 999, dto))
                        .ThrowsAsync(new KeyNotFoundException("Item not found in cart"));

            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => CreateController(1).UpdateItem(999, dto));
        }

        //  DELETE /api/cart/items/{productId} 

        [Fact]
        public async Task RemoveItem_ExistingItem_Returns200()
        {
            _serviceMock.Setup(s => s.RemoveItemAsync(1, 5)).ReturnsAsync(EmptyCart(1));

            var result = await CreateController(1).RemoveItem(5) as OkObjectResult;

            Assert.Equal(200, result?.StatusCode);
        }

        //  DELETE /api/cart 

        [Fact]
        public async Task ClearCart_Returns200WithEmptyCart()
        {
            _serviceMock.Setup(s => s.ClearCartAsync(1)).ReturnsAsync(EmptyCart(1));

            var result = await CreateController(1).ClearCart() as OkObjectResult;

            Assert.Equal(200, result?.StatusCode);
        }
    }
}