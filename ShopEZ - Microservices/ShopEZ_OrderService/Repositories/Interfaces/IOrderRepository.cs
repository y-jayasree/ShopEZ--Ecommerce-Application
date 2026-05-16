using ShopEZ_OrderService.Models;

namespace ShopEZ_OrderService.Repositories.Interfaces
{
    public interface IOrderRepository
    {
        Task<Order> CreateAsync(Order order);
        Task<Order?> GetByIdAsync(int orderId);
        Task<List<Order>> GetByUserIdAsync(int userId);
        Task<(List<Order> Orders, int Total)> GetAllAsync(int page, int pageSize);
        Task<Order> UpdateAsync(Order order);
    }
}