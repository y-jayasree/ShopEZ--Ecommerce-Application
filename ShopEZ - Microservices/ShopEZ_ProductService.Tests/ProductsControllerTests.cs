using Microsoft.AspNetCore.Mvc;
using Moq;
using ShopEZ_ProductService.Controllers;
using ShopEZ_ProductService.Services.Interfaces;
using ShopEZ_Shared_Lib.DTOs;
using Xunit;

namespace ShopEZ_ProductService.Tests.Controllers
{
    public class ProductsControllerTests
    {
        private readonly Mock<IProductService> _serviceMock = new();
        private ProductsController CreateController() => new(_serviceMock.Object);

        private static ProductResponseDTO SampleProduct() =>
            new() { ProductId = 1, Name = "Laptop", Price = 999m, CategoryId = 1, CategoryName = "Electronics" };

        [Fact]
        public async Task GetById_Found_Returns200()
        {
            _serviceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(SampleProduct());

            var result = await CreateController().GetById(1) as OkObjectResult;

            Assert.Equal(200, result?.StatusCode);
        }

        [Fact]
        public async Task GetById_NotFound_Returns404()
        {
            _serviceMock.Setup(s => s.GetByIdAsync(99)).ReturnsAsync((ProductResponseDTO?)null);

            var result = await CreateController().GetById(99);

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task Search_ValidKeyword_Returns200()
        {
            _serviceMock.Setup(s => s.SearchAsync("laptop"))
                        .ReturnsAsync(new List<ProductResponseDTO> { SampleProduct() });

            var result = await CreateController().Search("laptop") as OkObjectResult;

            Assert.Equal(200, result?.StatusCode);
        }

        [Fact]
        public async Task Search_EmptyKeyword_Returns400()
        {
            var result = await CreateController().Search("  ");

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Create_ValidDto_Returns201()
        {
            var dto = new CreateProductDTO { Name = "Monitor", Price = 299m, Stock = 5, CategoryId = 1 };
            _serviceMock.Setup(s => s.CreateAsync(dto, null)).ReturnsAsync(SampleProduct());

            var result = await CreateController().Create(dto, null) as CreatedAtActionResult;

            Assert.Equal(201, result?.StatusCode);
        }

        [Fact]
        public async Task Delete_ExistingProduct_Returns200()
        {
            _serviceMock.Setup(s => s.DeleteAsync(1)).Returns(Task.CompletedTask);

            var result = await CreateController().Delete(1) as OkObjectResult;

            Assert.Equal(200, result?.StatusCode);
        }
    }
}