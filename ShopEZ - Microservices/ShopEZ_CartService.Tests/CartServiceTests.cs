using ShopEZ_cartservice.Models;
using ShopEZ_cartservice.Repositories.Interfaces;
using ShopEZ_cartservice.Services;
using ShopEZ_Shared_Lib.DTOs;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;

namespace ShopEZ_CartService.Tests
{
    public class CartServiceTests
    {
        private readonly Mock<ICartRepository> _repoMock = new();
        private CartService CreateService() => new(_repoMock.Object);

        private static CartItem MakeItem(int userId, int productId, string name,
                                         decimal price, int qty) => new()
                                         {
                                             Id = productId * 10,
                                             UserId = userId,
                                             ProductId = productId,
                                             ProductName = name,
                                             UnitPrice = price,
                                             Quantity = qty
                                         };

        // ──────────────────────────────────────────────
        // GetCartAsync
        // ──────────────────────────────────────────────

        [Fact]
        public async Task GetCart_WithItems_ReturnsMappedResponse()
        {
            var items = new List<CartItem>
            {
                MakeItem(1, 101, "Widget", 9.99m, 2),
                MakeItem(1, 102, "Gadget", 14.99m, 1)
            };
            _repoMock.Setup(r => r.GetByUserIdAsync(1)).ReturnsAsync(items);

            var result = await CreateService().GetCartAsync(1);

            Assert.Equal(1, result.UserId);
            Assert.Equal(2, result.Items.Count);
            // TotalAmount = (9.99*2) + (14.99*1) = 34.97
            Assert.Equal(34.97m, result.TotalAmount);
        }

        [Fact]
        public async Task GetCart_EmptyCart_ReturnsEmptyItemList()
        {
            _repoMock.Setup(r => r.GetByUserIdAsync(5)).ReturnsAsync(new List<CartItem>());

            var result = await CreateService().GetCartAsync(5);

            Assert.Empty(result.Items);
            Assert.Equal(0m, result.TotalAmount);
        }

        // ──────────────────────────────────────────────
        // AddItemAsync — new item
        // ──────────────────────────────────────────────

        [Fact]
        public async Task AddItem_NewProduct_CreatesCartItem()
        {
            var dto = new AddToCartDTO { ProductId = 10, ProductName = "Pen", UnitPrice = 1.50m, Quantity = 3 };

            // No existing item
            _repoMock.Setup(r => r.GetItemAsync(1, 10)).ReturnsAsync((CartItem?)null);
            _repoMock.Setup(r => r.AddItemAsync(It.IsAny<CartItem>()))
                     .ReturnsAsync((CartItem c) => c);

            var updatedItems = new List<CartItem> { MakeItem(1, 10, "Pen", 1.50m, 3) };
            _repoMock.Setup(r => r.GetByUserIdAsync(1)).ReturnsAsync(updatedItems);

            var result = await CreateService().AddItemAsync(1, dto);

            // AddItemAsync must have been called exactly once
            _repoMock.Verify(r => r.AddItemAsync(It.Is<CartItem>(c =>
                c.ProductId == 10 &&
                c.ProductName == "Pen" &&
                c.Quantity == 3)), Times.Once);

            Assert.Single(result.Items);
        }

        // ──────────────────────────────────────────────
        // AddItemAsync — existing item (quantity merge)
        // ──────────────────────────────────────────────

        [Fact]
        public async Task AddItem_ExistingProduct_AccumulatesQuantity()
        {
            var existing = MakeItem(1, 10, "Pen", 1.50m, 2);   // already 2 in cart
            var dto = new AddToCartDTO { ProductId = 10, ProductName = "Pen", UnitPrice = 1.50m, Quantity = 3 };

            _repoMock.Setup(r => r.GetItemAsync(1, 10)).ReturnsAsync(existing);
            _repoMock.Setup(r => r.UpdateItemAsync(It.IsAny<CartItem>()))
                     .ReturnsAsync((CartItem c) => c);

            var updatedItems = new List<CartItem> { MakeItem(1, 10, "Pen", 1.50m, 5) };
            _repoMock.Setup(r => r.GetByUserIdAsync(1)).ReturnsAsync(updatedItems);

            var result = await CreateService().AddItemAsync(1, dto);

            // UpdateItemAsync must be called; quantity inside existing item should be 5
            _repoMock.Verify(r => r.UpdateItemAsync(It.Is<CartItem>(c =>
                c.Quantity == 5)), Times.Once);

            Assert.Equal(5, result.Items[0].Quantity);
        }

        // ──────────────────────────────────────────────
        // UpdateItemAsync
        // ──────────────────────────────────────────────

        [Fact]
        public async Task UpdateItem_ExistingItem_SetsNewQuantity()
        {
            var existing = MakeItem(1, 20, "Book", 12m, 1);
            var dto = new UpdateCartItemDTO { Quantity = 4 };

            _repoMock.Setup(r => r.GetItemAsync(1, 20)).ReturnsAsync(existing);
            _repoMock.Setup(r => r.UpdateItemAsync(It.IsAny<CartItem>()))
                     .ReturnsAsync((CartItem c) => c);

            var updatedItems = new List<CartItem> { MakeItem(1, 20, "Book", 12m, 4) };
            _repoMock.Setup(r => r.GetByUserIdAsync(1)).ReturnsAsync(updatedItems);

            var result = await CreateService().UpdateItemAsync(1, 20, dto);

            _repoMock.Verify(r => r.UpdateItemAsync(It.Is<CartItem>(c =>
                c.Quantity == 4)), Times.Once);

            Assert.Equal(4, result.Items[0].Quantity);
        }

        [Fact]
        public async Task UpdateItem_NonExistingItem_ThrowsKeyNotFoundException()
        {
            _repoMock.Setup(r => r.GetItemAsync(1, 999)).ReturnsAsync((CartItem?)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => CreateService().UpdateItemAsync(1, 999, new UpdateCartItemDTO { Quantity = 1 }));
        }

        // ──────────────────────────────────────────────
        // RemoveItemAsync
        // ──────────────────────────────────────────────

        [Fact]
        public async Task RemoveItem_ExistingItem_RemovesAndReturnsUpdatedCart()
        {
            _repoMock.Setup(r => r.RemoveItemAsync(1, 30)).ReturnsAsync(true);
            _repoMock.Setup(r => r.GetByUserIdAsync(1)).ReturnsAsync(new List<CartItem>());

            var result = await CreateService().RemoveItemAsync(1, 30);

            Assert.Empty(result.Items);
            _repoMock.Verify(r => r.RemoveItemAsync(1, 30), Times.Once);
        }

        [Fact]
        public async Task RemoveItem_NonExistingItem_ThrowsKeyNotFoundException()
        {
            _repoMock.Setup(r => r.RemoveItemAsync(1, 404)).ReturnsAsync(false);

            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => CreateService().RemoveItemAsync(1, 404));
        }

        // ──────────────────────────────────────────────
        // ClearCartAsync
        // ──────────────────────────────────────────────

        [Fact]
        public async Task ClearCart_ClearsAllItems_ReturnsEmptyCart()
        {
            _repoMock.Setup(r => r.ClearCartAsync(1)).Returns(Task.CompletedTask);

            var result = await CreateService().ClearCartAsync(1);

            _repoMock.Verify(r => r.ClearCartAsync(1), Times.Once);
            Assert.Empty(result.Items);
            Assert.Equal(0m, result.TotalAmount);
        }

        // ──────────────────────────────────────────────
        // TotalAmount calculation (edge cases)
        // ──────────────────────────────────────────────

        [Fact]
        public async Task GetCart_TotalAmountIsCorrect_MultipleItems()
        {
            var items = new List<CartItem>
            {
                MakeItem(2, 1, "A", 10.00m, 3),   // 30
                MakeItem(2, 2, "B",  5.50m, 2),   // 11
                MakeItem(2, 3, "C",  0.99m, 10)   //  9.90
            };
            _repoMock.Setup(r => r.GetByUserIdAsync(2)).ReturnsAsync(items);

            var result = await CreateService().GetCartAsync(2);

            Assert.Equal(50.90m, result.TotalAmount);
        }
    }
}
    
