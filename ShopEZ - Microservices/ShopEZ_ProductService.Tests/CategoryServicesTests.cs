using Moq;
using ShopEZ_ProductService.Models;
using ShopEZ_ProductService.Repositories.Interfaces;
using ShopEZ_ProductService.Services;
using ShopEZ_Shared_Lib.DTOs;
using Xunit;

namespace ShopEZ_ProductService.Tests.Services
{
    public class CategoryServiceTests
    {
        private readonly Mock<ICategoryRepository> _repoMock = new();
        private CategoryService CreateService() => new(_repoMock.Object);

        //  GetAll 

        [Fact]
        public async Task GetAll_ReturnsMappedList()
        {
            _repoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Category>
            {
                new() { CategoryId = 1, Name = "Electronics" },
                new() { CategoryId = 2, Name = "Clothing" }
            });

            var result = await CreateService().GetAllAsync();

            Assert.Equal(2, result.Count);
        }

        //  Create 

        [Fact]
        public async Task Create_ValidDto_ReturnsCategoryResponse()
        {
            var dto = new CreateCategoryDTO { Name = "Books" };
            var created = new Category { CategoryId = 3, Name = "Books" };

            _repoMock.Setup(r => r.NameExistsAsync("Books", null)).ReturnsAsync(false);
            _repoMock.Setup(r => r.CreateAsync(It.IsAny<Category>())).ReturnsAsync(created);

            var result = await CreateService().CreateAsync(dto);

            Assert.Equal("Books", result.Name);
        }

        [Fact]
        public async Task Create_DuplicateName_ThrowsInvalidOperationException()
        {
            _repoMock.Setup(r => r.NameExistsAsync("Electronics", null)).ReturnsAsync(true);

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => CreateService().CreateAsync(new CreateCategoryDTO { Name = "Electronics" }));
        }

        //  Update 

        [Fact]
        public async Task Update_ExistingCategory_ReturnsUpdatedResponse()
        {
            var dto = new CreateCategoryDTO { Name = "Updated", Description = "New desc" };
            var existing = new Category { CategoryId = 1, Name = "Electronics" };
            var updated = new Category { CategoryId = 1, Name = "Updated", Description = "New desc" };

            _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existing);
            _repoMock.Setup(r => r.NameExistsAsync("Updated", 1)).ReturnsAsync(false);
            _repoMock.Setup(r => r.UpdateAsync(It.IsAny<Category>())).ReturnsAsync(updated);

            var result = await CreateService().UpdateAsync(1, dto);

            Assert.Equal("Updated", result.Name);
        }

        [Fact]
        public async Task Update_NotFound_ThrowsKeyNotFoundException()
        {
            _repoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Category?)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => CreateService().UpdateAsync(99, new CreateCategoryDTO { Name = "X" }));
        }

        // Delete 

        [Fact]
        public async Task Delete_ExistingCategory_Succeeds()
        {
            _repoMock.Setup(r => r.DeleteAsync(1)).ReturnsAsync(true);

            await CreateService().DeleteAsync(1);

            _repoMock.Verify(r => r.DeleteAsync(1), Times.Once);
        }

        [Fact]
        public async Task Delete_NotFound_ThrowsKeyNotFoundException()
        {
            _repoMock.Setup(r => r.DeleteAsync(99)).ReturnsAsync(false);

            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => CreateService().DeleteAsync(99));
        }
    }
}