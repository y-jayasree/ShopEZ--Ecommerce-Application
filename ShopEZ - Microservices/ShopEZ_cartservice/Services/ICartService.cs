using ShopEZ_Shared_Lib.DTOs;

namespace ShopEZ_cartservice.Services.Interfaces
{
    public interface ICartService
    {
        Task<CartResponseDTO> GetCartAsync(int userId);
        Task<CartResponseDTO> AddItemAsync(int userId, AddToCartDTO dto);
        Task<CartResponseDTO> UpdateItemAsync(int userId, int productId, UpdateCartItemDTO dto);
        Task<CartResponseDTO> RemoveItemAsync(int userId, int productId);
        Task<CartResponseDTO> ClearCartAsync(int userId);
    }
}