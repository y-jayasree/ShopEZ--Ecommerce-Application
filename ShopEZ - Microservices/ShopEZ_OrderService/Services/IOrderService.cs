using ShopEZ_Shared_Lib.DTOs;

namespace ShopEZ_OrderService.Services
{
    public interface IOrderService
    {
        Task<OrderResponseDTO> PlaceOrderAsync(int userId, CreateOrderDTO dto);
        Task<List<OrderResponseDTO>> GetMyOrdersAsync(int userId);
        Task<OrderResponseDTO?> GetByIdAsync(int orderId);
        Task<(List<OrderResponseDTO> Orders, int Total)> GetAllOrdersAsync(int page, int pageSize);
        Task<OrderResponseDTO> UpdateStatusAsync(int orderId, string status);
    }
}