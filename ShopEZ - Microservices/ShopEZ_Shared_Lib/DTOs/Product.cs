using System.ComponentModel.DataAnnotations;

namespace ShopEZ_Shared_Lib.DTOs
{
    public class CreateProductDTO
    {
        [Required(ErrorMessage = "Product name is required")]
        [StringLength(200, MinimumLength = 2)]
        public string Name { get; set; } = "";

        [StringLength(2000)]
        public string? Description { get; set; }

        [Required]
        [Range(0.01, 999999.99, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Stock cannot be negative")]
        public int Stock { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Valid category is required")]
        public int CategoryId { get; set; }
    }

    public class UpdateProductDTO
    {
        [StringLength(200, MinimumLength = 2)]
        public string? Name { get; set; }

        [StringLength(2000)]
        public string? Description { get; set; }

        [Range(0.01, 999999.99)]
        public decimal? Price { get; set; }

        [Range(0, int.MaxValue)]
        public int? Stock { get; set; }

        public int? CategoryId { get; set; }
    }

    public class CreateCategoryDTO
    {
        [Required(ErrorMessage = "Category name is required")]
        [StringLength(100, MinimumLength = 2)]
        public string Name { get; set; } = "";

        [StringLength(500)]
        public string? Description { get; set; }
    }

    public class ProductResponseDTO
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = "";
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public string? ImageUrl { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = "";
        public DateTime CreatedAt { get; set; }
    }

    public class PagedProductResponseDTO
    {
        public List<ProductResponseDTO> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    }

    public class CategoryResponseDTO
    {
        public int CategoryId { get; set; }
        public string Name { get; set; } = "";
        public string? Description { get; set; }
    }
}