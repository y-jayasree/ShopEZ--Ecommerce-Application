using ShopEZ_cartservice.Models;

namespace ShopEZ_cartservice.Repositories.Interfaces
{
    public interface ICartRepository
    {
        Task<List<CartItem>> GetByUserIdAsync(int userId);
        Task<CartItem?> GetItemAsync(int userId, int productId);
        Task<CartItem> AddItemAsync(CartItem item);
        Task<CartItem> UpdateItemAsync(CartItem item);
        Task<bool> RemoveItemAsync(int userId, int productId);
        Task ClearCartAsync(int userId);
    }
}