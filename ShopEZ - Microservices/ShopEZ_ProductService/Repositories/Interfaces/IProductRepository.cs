using ShopEZ_ProductService.Models;
using ShopEZ_Shared_Lib.DTOs;

namespace ShopEZ_ProductService.Repositories.Interfaces
{
    public interface IProductRepository
    {
        Task<PagedProductResponseDTO> GetPagedAsync(
            string? keyword, int? categoryId, decimal? minPrice, decimal? maxPrice,
            string? sortBy, int page, int pageSize);

        Task<ProductResponseDTO?> GetByIdAsync(int id);
        Task<List<ProductResponseDTO>> SearchAsync(string keyword);
        Task<Product> CreateAsync(Product product);
        Task<Product> UpdateAsync(Product product);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<Product?> GetEntityAsync(int id);
    }
}