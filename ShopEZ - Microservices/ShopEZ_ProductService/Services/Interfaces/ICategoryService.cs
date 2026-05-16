using ShopEZ_Shared_Lib.DTOs;

namespace ShopEZ_ProductService.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<List<CategoryResponseDTO>> GetAllAsync();
        Task<CategoryResponseDTO> CreateAsync(CreateCategoryDTO dto);
        Task<CategoryResponseDTO> UpdateAsync(int id, CreateCategoryDTO dto);
        Task DeleteAsync(int id);
    }
}
