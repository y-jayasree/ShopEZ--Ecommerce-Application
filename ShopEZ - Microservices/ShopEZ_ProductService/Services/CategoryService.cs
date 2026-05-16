using ShopEZ_ProductService.Models;
using ShopEZ_ProductService.Repositories.Interfaces;
using ShopEZ_ProductService.Services.Interfaces;
using ShopEZ_Shared_Lib.DTOs;

namespace ShopEZ_ProductService.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categories;

        public CategoryService(ICategoryRepository categories)
        {
            _categories = categories;
        }

        public async Task<List<CategoryResponseDTO>> GetAllAsync()
        {
            var list = await _categories.GetAllAsync();
            return list.Select(c => new CategoryResponseDTO
            {
                CategoryId = c.CategoryId,
                Name = c.Name,
                Description = c.Description
            }).ToList();
        }

        public async Task<CategoryResponseDTO> CreateAsync(CreateCategoryDTO dto)
        {
            if (await _categories.NameExistsAsync(dto.Name))
                throw new InvalidOperationException($"Category '{dto.Name}' already exists");

            var category = new Category
            {
                Name = dto.Name.Trim(),
                Description = dto.Description?.Trim()
            };

            var created = await _categories.CreateAsync(category);

            return new CategoryResponseDTO
            {
                CategoryId = created.CategoryId,
                Name = created.Name,
                Description = created.Description
            };
        }

        public async Task<CategoryResponseDTO> UpdateAsync(int id, CreateCategoryDTO dto)
        {
            var category = await _categories.GetByIdAsync(id)
                ?? throw new KeyNotFoundException("Category not found");

            if (await _categories.NameExistsAsync(dto.Name, id))
                throw new InvalidOperationException($"Category name '{dto.Name}' is already taken");

            category.Name = dto.Name.Trim();
            category.Description = dto.Description?.Trim();

            var updated = await _categories.UpdateAsync(category);

            return new CategoryResponseDTO
            {
                CategoryId = updated.CategoryId,
                Name = updated.Name,
                Description = updated.Description
            };
        }

        public async Task DeleteAsync(int id)
        {
            var deleted = await _categories.DeleteAsync(id);
            if (!deleted)
                throw new KeyNotFoundException("Category not found");
        }
    }
}