using Microsoft.AspNetCore.Mvc;
using Moq;
using ShopEZ_ProductService.Controllers;
using ShopEZ_ProductService.Services.Interfaces;
using ShopEZ_Shared_Lib.DTOs;
using Xunit;

namespace ShopEZ_ProductService.Tests.Controllers
{
    public class CategoriesControllerTests
    {
        private readonly Mock<ICategoryService> _serviceMock = new();
        private CategoriesController CreateController() => new(_serviceMock.Object);

        private static CategoryResponseDTO SampleCategory() =>
            new() { CategoryId = 1, Name = "Electronics" };

        [Fact]
        public async Task GetAll_Returns200WithList()
        {
            _serviceMock.Setup(s => s.GetAllAsync())
                        .ReturnsAsync(new List<CategoryResponseDTO> { SampleCategory() });

            var result = await CreateController().GetAll() as OkObjectResult;

            Assert.Equal(200, result?.StatusCode);
        }

        [Fact]
        public async Task Create_ValidDto_Returns201()
        {
            var dto = new CreateCategoryDTO { Name = "Books" };
            _serviceMock.Setup(s => s.CreateAsync(dto)).ReturnsAsync(SampleCategory());

            var result = await CreateController().Create(dto) as ObjectResult;

            Assert.Equal(201, result?.StatusCode);
        }

        [Fact]
        public async Task Update_ExistingCategory_Returns200()
        {
            var dto = new CreateCategoryDTO { Name = "Updated" };
            _serviceMock.Setup(s => s.UpdateAsync(1, dto)).ReturnsAsync(SampleCategory());

            var result = await CreateController().Update(1, dto) as OkObjectResult;

            Assert.Equal(200, result?.StatusCode);
        }

        [Fact]
        public async Task Delete_ExistingCategory_Returns200()
        {
            _serviceMock.Setup(s => s.DeleteAsync(1)).Returns(Task.CompletedTask);

            var result = await CreateController().Delete(1) as OkObjectResult;

            Assert.Equal(200, result?.StatusCode);
        }
    }
}