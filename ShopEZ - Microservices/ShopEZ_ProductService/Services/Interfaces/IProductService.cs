using ShopEZ_Shared_Lib.DTOs;

namespace ShopEZ_ProductService.Services.Interfaces
{
    public interface IProductService
    {
        Task<PagedProductResponseDTO> GetProductsAsync(string? keyword, int? categoryId, decimal? minPrice, decimal? maxPrice, string? sortBy, int page, int pageSize);
        Task<ProductResponseDTO?> GetByIdAsync(int id);
        Task<List<ProductResponseDTO>> SearchAsync(string keyword);
        Task<ProductResponseDTO> CreateAsync(CreateProductDTO dto, IFormFile? imageFile);
        Task<ProductResponseDTO> UpdateAsync(int id, UpdateProductDTO dto, IFormFile? imageFile);
        Task DeleteAsync(int id);
    }

}