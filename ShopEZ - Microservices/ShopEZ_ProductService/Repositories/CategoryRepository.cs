using Microsoft.EntityFrameworkCore;
using ShopEZ_ProductService.Data;
using ShopEZ_ProductService.Models;
using ShopEZ_ProductService.Repositories.Interfaces;

namespace ShopEZ_ProductService.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly ProductDbContext _db;

        public CategoryRepository(ProductDbContext db)
        {
            _db = db;
        }

        public async Task<List<Category>> GetAllAsync()
        {
            return await _db.Categories
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<Category?> GetByIdAsync(int id)
        {
            return await _db.Categories.FindAsync(id);
        }

        public async Task<bool> NameExistsAsync(string name, int? excludeId = null)
        {
            return await _db.Categories.AnyAsync(c =>
                c.Name == name && c.IsActive && (excludeId == null || c.CategoryId != excludeId));
        }

        public async Task<Category> CreateAsync(Category category)
        {
            _db.Categories.Add(category);
            await _db.SaveChangesAsync();
            return category;
        }

        public async Task<Category> UpdateAsync(Category category)
        {
            _db.Categories.Update(category);
            await _db.SaveChangesAsync();
            return category;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var category = await _db.Categories.FindAsync(id);
            if (category == null) return false;

            var hasProducts = await _db.Products.AnyAsync(p => p.CategoryId == id && p.IsActive);
            if (hasProducts)
                throw new InvalidOperationException("Cannot delete category with active products");

            category.IsActive = false;
            await _db.SaveChangesAsync();
            return true;
        }
    }
}