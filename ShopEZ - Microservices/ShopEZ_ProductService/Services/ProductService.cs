using ShopEZ_ProductService.Models;
using ShopEZ_ProductService.Repositories.Interfaces;
using ShopEZ_ProductService.Services.Interfaces;
using ShopEZ_Shared_Lib.DTOs;

namespace ShopEZ_ProductService.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _products;
        private readonly ICategoryRepository _categories;
        private readonly IWebHostEnvironment _env;

        public ProductService(IProductRepository products, ICategoryRepository categories, IWebHostEnvironment env)
        {
            _products = products;
            _categories = categories;
            _env = env;
        }

        public async Task<PagedProductResponseDTO> GetProductsAsync(
            string? keyword, int? categoryId, decimal? minPrice, decimal? maxPrice,
            string? sortBy, int page, int pageSize)
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 100);
            return await _products.GetPagedAsync(keyword, categoryId, minPrice, maxPrice, sortBy, page, pageSize);
        }

        public async Task<ProductResponseDTO?> GetByIdAsync(int id)
        {
            return await _products.GetByIdAsync(id);
        }

        public async Task<List<ProductResponseDTO>> SearchAsync(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return new List<ProductResponseDTO>();
            return await _products.SearchAsync(keyword.Trim());
        }

        public async Task<ProductResponseDTO> CreateAsync(CreateProductDTO dto, IFormFile? imageFile)
        {
            var category = await _categories.GetByIdAsync(dto.CategoryId)
                ?? throw new KeyNotFoundException("Category not found");

            var product = new Product
            {
                Name = dto.Name.Trim(),
                Description = dto.Description?.Trim(),
                Price = dto.Price,
                Stock = dto.Stock,
                CategoryId = dto.CategoryId,
                ImageUrl = imageFile != null ? await SaveImageAsync(imageFile) : null
            };

            var created = await _products.CreateAsync(product);

            return new ProductResponseDTO
            {
                ProductId = created.ProductId,
                Name = created.Name,
                Description = created.Description,
                Price = created.Price,
                Stock = created.Stock,
                ImageUrl = created.ImageUrl,
                CategoryId = created.CategoryId,
                CategoryName = category.Name,
                CreatedAt = created.CreatedAt
            };
        }

        public async Task<ProductResponseDTO> UpdateAsync(int id, UpdateProductDTO dto, IFormFile? imageFile)
        {
            // _products.GetEntityAsync() through the interface instead of
            // casting to the concrete ProductRepository — which broke abstraction
            // and would throw InvalidCastException if repo was mocked or swapped.
            var dbProduct = await _products.GetEntityAsync(id)
                ?? throw new KeyNotFoundException("Product not found");

            if (dto.Name != null) dbProduct.Name = dto.Name.Trim();
            if (dto.Description != null) dbProduct.Description = dto.Description.Trim();
            if (dto.Price.HasValue) dbProduct.Price = dto.Price.Value;
            if (dto.Stock.HasValue) dbProduct.Stock = dto.Stock.Value;

            if (dto.CategoryId.HasValue)
            {
                _ = await _categories.GetByIdAsync(dto.CategoryId.Value)
                    ?? throw new KeyNotFoundException("Category not found");
                dbProduct.CategoryId = dto.CategoryId.Value;
            }

            if (imageFile != null)
                dbProduct.ImageUrl = await SaveImageAsync(imageFile);

            await _products.UpdateAsync(dbProduct);

            return await _products.GetByIdAsync(dbProduct.ProductId)
                ?? throw new InvalidOperationException("Failed to fetch updated product");
        }

        public async Task DeleteAsync(int id)
        {
            var deleted = await _products.DeleteAsync(id);
            if (!deleted)
                throw new KeyNotFoundException("Product not found");
        }

        private async Task<string> SaveImageAsync(IFormFile file)
        {
            var mimeType = file.ContentType.ToLower().Split(';')[0].Trim();
            var allowed = new[] { "image/jpeg", "image/png", "image/webp" };
            if (!allowed.Contains(mimeType))
                throw new ArgumentException("Only JPG, PNG and WebP images are allowed");

            if (file.Length > 5 * 1024 * 1024)
                throw new ArgumentException("Image cannot exceed 5MB");

            var folder = Path.Combine(_env.WebRootPath ?? "wwwroot", "images", "products");
            Directory.CreateDirectory(folder);

            var ext = Path.GetExtension(file.FileName).ToLower();
            var fileName = $"{Guid.NewGuid()}{ext}";
            var path = Path.Combine(folder, fileName);

            await using var stream = new FileStream(path, FileMode.Create);
            await file.CopyToAsync(stream);

            return $"/images/products/{fileName}";
        }
    }
}