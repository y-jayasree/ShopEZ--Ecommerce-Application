using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopEZ_ProductService.Services.Interfaces;
using ShopEZ_Shared_Lib.DTOs;
using ShopEZ_Shared_Lib.Helpers;

namespace ShopEZ_ProductService.Controllers
{
    [ApiController]
    [Route("api/categories")]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var categories = await _categoryService.GetAllAsync();
            return Ok(ApiResponse<List<CategoryResponseDTO>>.Ok(categories, "Categories retrieved", categories.Count));
        }

        [Authorize(Roles = "ADMIN")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCategoryDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Fail(GetModelErrors()));

            var created = await _categoryService.CreateAsync(dto);
            return StatusCode(201, ApiResponse<CategoryResponseDTO>.Ok(created, "Category created"));
        }

        [Authorize(Roles = "ADMIN")]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] CreateCategoryDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Fail(GetModelErrors()));

            var updated = await _categoryService.UpdateAsync(id, dto);
            return Ok(ApiResponse<CategoryResponseDTO>.Ok(updated, "Category updated"));
        }

        [Authorize(Roles = "ADMIN")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _categoryService.DeleteAsync(id);
            return Ok(ApiResponse<object>.Ok(new { }, "Category deleted"));
        }

        private string GetModelErrors()
        {
            return string.Join("; ", ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage));
        }
    }
}