using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopEZ_ProductService.Services.Interfaces;
using ShopEZ_Shared_Lib.DTOs;
using ShopEZ_Shared_Lib.Helpers;

namespace ShopEZ_ProductService.Controllers
{
    [ApiController]
    [Route("api/products")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet]
        public async Task<IActionResult> GetProducts(
            [FromQuery] string? keyword,
            [FromQuery] int? categoryId,
            [FromQuery] decimal? minPrice,
            [FromQuery] decimal? maxPrice,
            [FromQuery] string? sortBy,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var result = await _productService.GetProductsAsync(keyword, categoryId, minPrice, maxPrice, sortBy, page, pageSize);
            return Ok(ApiResponse<PagedProductResponseDTO>.Ok(result, "Products retrieved", result.TotalCount));
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var product = await _productService.GetByIdAsync(id);
            if (product == null)
                return NotFound(ApiResponse<object>.Fail("Product not found"));

            return Ok(ApiResponse<ProductResponseDTO>.Ok(product));
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return BadRequest(ApiResponse<object>.Fail("Search keyword is required"));

            var results = await _productService.SearchAsync(keyword);
            return Ok(ApiResponse<List<ProductResponseDTO>>.Ok(results, "Search results", results.Count));
        }

        [Authorize(Roles = "ADMIN")]
        [HttpPost]
        public async Task<IActionResult> Create([FromForm] CreateProductDTO dto, IFormFile? imageFile)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Fail(GetModelErrors()));

            var created = await _productService.CreateAsync(dto, imageFile);
            return CreatedAtAction(nameof(GetById), new { id = created.ProductId },
                ApiResponse<ProductResponseDTO>.Ok(created, "Product created"));
        }

        [Authorize(Roles = "ADMIN")]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromForm] UpdateProductDTO dto, IFormFile? imageFile)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Fail(GetModelErrors()));

            var updated = await _productService.UpdateAsync(id, dto, imageFile);
            return Ok(ApiResponse<ProductResponseDTO>.Ok(updated, "Product updated"));
        }

        [Authorize(Roles = "ADMIN")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _productService.DeleteAsync(id);
            return Ok(ApiResponse<object>.Ok(new { }, "Product deleted"));
        }

        private string GetModelErrors()
        {
            return string.Join("; ", ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage));
        }
    }
}