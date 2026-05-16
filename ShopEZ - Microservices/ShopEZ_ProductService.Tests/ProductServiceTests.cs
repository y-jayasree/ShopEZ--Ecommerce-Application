using Microsoft.AspNetCore.Hosting;
using Moq;
using ShopEZ_ProductService.Models;
using ShopEZ_ProductService.Repositories.Interfaces;
using ShopEZ_ProductService.Services;
using ShopEZ_Shared_Lib.DTOs;
using Xunit;

namespace ShopEZ_ProductService.Tests.Services
{
    public class ProductServiceTests
    {
        private readonly Mock<IProductRepository> _productRepoMock = new();
        private readonly Mock<ICategoryRepository> _categoryRepoMock = new();
        private readonly Mock<IWebHostEnvironment> _envMock = new();

        private ProductService CreateService() =>
            new(_productRepoMock.Object, _categoryRepoMock.Object, _envMock.Object);

        //  GetById 

        [Fact]
        public async Task GetById_ExistingProduct_ReturnsResponse()
        {
            var response = new ProductResponseDTO { ProductId = 1, Name = "Laptop", Price = 999m };
            _productRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(response);

            var result = await CreateService().GetByIdAsync(1);

            Assert.NotNull(result);
            Assert.Equal("Laptop", result!.Name);
        }

        [Fact]
        public async Task GetById_NotFound_ReturnsNull()
        {
            _productRepoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((ProductResponseDTO?)null);

            var result = await CreateService().GetByIdAsync(99);

            Assert.Null(result);
        }

        //  Search 

        [Fact]
        public async Task Search_ValidKeyword_ReturnsResults()
        {
            var products = new List<ProductResponseDTO>
            {
                new() { ProductId = 1, Name = "Laptop Pro" },
                new() { ProductId = 2, Name = "Laptop Air" }
            };
            _productRepoMock.Setup(r => r.SearchAsync("laptop")).ReturnsAsync(products);

            var result = await CreateService().SearchAsync("laptop");

            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task Search_EmptyKeyword_ReturnsEmptyWithoutCallingRepo()
        {
            var result = await CreateService().SearchAsync("  ");

            Assert.Empty(result);
            _productRepoMock.Verify(r => r.SearchAsync(It.IsAny<string>()), Times.Never);
        }

        //  Create 

        [Fact]
        public async Task Create_ValidDto_ReturnsCreatedProduct()
        {
            var dto = new CreateProductDTO { Name = "Mouse", Price = 29.99m, Stock = 50, CategoryId = 1 };
            var category = new Category { CategoryId = 1, Name = "Accessories" };
            var saved = new Product { ProductId = 5, Name = "Mouse", Price = 29.99m, Stock = 50, CategoryId = 1 };

            _categoryRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(category);
            _productRepoMock.Setup(r => r.CreateAsync(It.IsAny<Product>())).ReturnsAsync(saved);

            var result = await CreateService().CreateAsync(dto, null);

            Assert.Equal("Mouse", result.Name);
            Assert.Equal("Accessories", result.CategoryName);
        }

        [Fact]
        public async Task Create_InvalidCategory_ThrowsKeyNotFoundException()
        {
            var dto = new CreateProductDTO { Name = "Ghost", Price = 10m, Stock = 1, CategoryId = 999 };
            _categoryRepoMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Category?)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => CreateService().CreateAsync(dto, null));
        }

        //  Update 

        [Fact]
        public async Task Update_ExistingProduct_ReturnsUpdatedProduct()
        {
            var dto = new UpdateProductDTO { Price = 59.99m };
            var existing = new Product { ProductId = 3, Name = "Keyboard", Price = 49m, Stock = 10, CategoryId = 1 };
            var response = new ProductResponseDTO { ProductId = 3, Name = "Keyboard", Price = 59.99m };

            _productRepoMock.Setup(r => r.GetEntityAsync(3)).ReturnsAsync(existing);
            _productRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Product>())).ReturnsAsync(existing);
            _productRepoMock.Setup(r => r.GetByIdAsync(3)).ReturnsAsync(response);

            var result = await CreateService().UpdateAsync(3, dto, null);

            Assert.Equal(59.99m, result.Price);
        }

        [Fact]
        public async Task Update_NotFound_ThrowsKeyNotFoundException()
        {
            _productRepoMock.Setup(r => r.GetEntityAsync(99)).ReturnsAsync((Product?)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => CreateService().UpdateAsync(99, new UpdateProductDTO(), null));
        }

        //  Delete 

        [Fact]
        public async Task Delete_ExistingProduct_Succeeds()
        {
            _productRepoMock.Setup(r => r.DeleteAsync(1)).ReturnsAsync(true);

            await CreateService().DeleteAsync(1);

            _productRepoMock.Verify(r => r.DeleteAsync(1), Times.Once);
        }

        [Fact]
        public async Task Delete_NotFound_ThrowsKeyNotFoundException()
        {
            _productRepoMock.Setup(r => r.DeleteAsync(99)).ReturnsAsync(false);

            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => CreateService().DeleteAsync(99));
        }
    }
}