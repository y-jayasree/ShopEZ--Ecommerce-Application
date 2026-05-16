using ShopEZ_cartservice.Models;
using ShopEZ_cartservice.Repositories.Interfaces;
using ShopEZ_cartservice.Services.Interfaces;
using ShopEZ_Shared_Lib.DTOs;

namespace ShopEZ_cartservice.Services
{
    public class CartService : ICartService
    {
        private readonly ICartRepository _cart;

        public CartService(ICartRepository cart)
        {
            _cart = cart;
        }

        public async Task<CartResponseDTO> GetCartAsync(int userId)
        {
            var items = await _cart.GetByUserIdAsync(userId);
            return MapToResponse(userId, items);
        }

        public async Task<CartResponseDTO> AddItemAsync(int userId, AddToCartDTO dto)
        {
            var existing = await _cart.GetItemAsync(userId, dto.ProductId);

            if (existing != null)
            {
                existing.Quantity += dto.Quantity;
                existing.UnitPrice = dto.UnitPrice;
                await _cart.UpdateItemAsync(existing);
            }
            else
            {
                var newItem = new CartItem
                {
                    UserId = userId,
                    ProductId = dto.ProductId,
                    ProductName = dto.ProductName,
                    UnitPrice = dto.UnitPrice,
                    Quantity = dto.Quantity,
                    ImageUrl = dto.ImageUrl,
                    AddedAt = DateTime.UtcNow
                };
                await _cart.AddItemAsync(newItem);
            }

            var items = await _cart.GetByUserIdAsync(userId);
            return MapToResponse(userId, items);
        }

        public async Task<CartResponseDTO> UpdateItemAsync(int userId, int productId, UpdateCartItemDTO dto)
        {
            var item = await _cart.GetItemAsync(userId, productId)
                ?? throw new KeyNotFoundException("Item not found in cart");

            item.Quantity = dto.Quantity;
            await _cart.UpdateItemAsync(item);

            var items = await _cart.GetByUserIdAsync(userId);
            return MapToResponse(userId, items);
        }

        public async Task<CartResponseDTO> RemoveItemAsync(int userId, int productId)
        {
            var removed = await _cart.RemoveItemAsync(userId, productId);
            if (!removed)
                throw new KeyNotFoundException("Item not found in cart");

            var items = await _cart.GetByUserIdAsync(userId);
            return MapToResponse(userId, items);
        }

        public async Task<CartResponseDTO> ClearCartAsync(int userId)
        {
            await _cart.ClearCartAsync(userId);
            return MapToResponse(userId, new List<CartItem>());
        }

        private static CartResponseDTO MapToResponse(int userId, List<CartItem> items)
        {
            return new CartResponseDTO
            {
                UserId = userId,
                Items = items.Select(i => new CartItemResponseDTO
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    ProductName = i.ProductName,
                    UnitPrice = i.UnitPrice,
                    Quantity = i.Quantity,
                    ImageUrl = i.ImageUrl
                }).ToList()
            };
        }
    }
}