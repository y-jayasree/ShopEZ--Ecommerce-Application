using Microsoft.EntityFrameworkCore;
using ShopEZ_OrderService.Data;
using ShopEZ_OrderService.Models;
using ShopEZ_OrderService.Repositories.Interfaces;

namespace ShopEZ_OrderService.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly OrderDbContext _db;

        public OrderRepository(OrderDbContext db)
        {
            _db = db;
        }

        public async Task<Order> CreateAsync(Order order)
        {
            _db.Orders.Add(order);
            await _db.SaveChangesAsync();
            return order;
        }

        public async Task<Order?> GetByIdAsync(int orderId)
        {
            return await _db.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);
        }

        public async Task<List<Order>> GetByUserIdAsync(int userId)
        {
            return await _db.Orders
                .Include(o => o.Items)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task<(List<Order> Orders, int Total)> GetAllAsync(int page, int pageSize)
        {
            var skip = (page - 1) * pageSize;

            var orders = await _db.Orders
                .Include(o => o.Items)
                .OrderByDescending(o => o.OrderDate)
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();

            var total = await _db.Orders.CountAsync();
            return (orders, total);
        }

        public async Task<Order> UpdateAsync(Order order)
        {
            _db.Orders.Update(order);
            await _db.SaveChangesAsync();
            return order;
        }
    }
}