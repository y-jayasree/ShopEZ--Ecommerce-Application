using Microsoft.EntityFrameworkCore;
using ShopEZ_cartservice.Data;
using ShopEZ_cartservice.Models;
using ShopEZ_cartservice.Repositories.Interfaces;

namespace ShopEZ_cartservice.Repositories
{
    public class CartRepository : ICartRepository
    {
        private readonly CartDbContext _db;

        public CartRepository(CartDbContext db)
        {
            _db = db;
        }

        public async Task<List<CartItem>> GetByUserIdAsync(int userId)
        {
            return await _db.CartItems
                .Where(c => c.UserId == userId)
                .OrderBy(c => c.AddedAt)
                .ToListAsync();
        }

        public async Task<CartItem?> GetItemAsync(int userId, int productId)
        {
            return await _db.CartItems
                .FirstOrDefaultAsync(c => c.UserId == userId && c.ProductId == productId);
        }

        public async Task<CartItem> AddItemAsync(CartItem item)
        {
            _db.CartItems.Add(item);
            await _db.SaveChangesAsync();
            return item;
        }

        public async Task<CartItem> UpdateItemAsync(CartItem item)
        {
            item.UpdatedAt = DateTime.UtcNow;
            _db.CartItems.Update(item);
            await _db.SaveChangesAsync();
            return item;
        }

        public async Task<bool> RemoveItemAsync(int userId, int productId)
        {
            var item = await GetItemAsync(userId, productId);
            if (item == null) return false;

            _db.CartItems.Remove(item);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task ClearCartAsync(int userId)
        {
            var items = await _db.CartItems.Where(c => c.UserId == userId).ToListAsync();
            _db.CartItems.RemoveRange(items);
            await _db.SaveChangesAsync();
        }
    }
}