using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ShopEZ_ProductService.Data;
using ShopEZ_ProductService.Models;
using ShopEZ_ProductService.Repositories.Interfaces;
using ShopEZ_Shared_Lib.DTOs;
using System.Text;

namespace ShopEZ_ProductService.Repositories
{
    // Reads use Dapper for speed. Writes use EF Core for safety/migrations.
    public class ProductRepository : IProductRepository
    {
        private readonly ProductDbContext _db;
        private readonly string _connectionString;

        public ProductRepository(ProductDbContext db, IConfiguration config)
        {
            _db = db;
            _connectionString = config.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string not configured");
        }

        private SqlConnection GetConnection() => new SqlConnection(_connectionString);

        public async Task<PagedProductResponseDTO> GetPagedAsync(
            string? keyword, int? categoryId, decimal? minPrice, decimal? maxPrice,
            string? sortBy, int page, int pageSize)
        {
            var where = new StringBuilder("WHERE p.IsActive = 1");
            var parameters = new DynamicParameters();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                where.Append(" AND (p.Name LIKE @Keyword OR p.Description LIKE @Keyword)");
                parameters.Add("Keyword", $"%{keyword}%");
            }
            if (categoryId.HasValue)
            {
                where.Append(" AND p.CategoryId = @CategoryId");
                parameters.Add("CategoryId", categoryId.Value);
            }
            if (minPrice.HasValue)
            {
                where.Append(" AND p.Price >= @MinPrice");
                parameters.Add("MinPrice", minPrice.Value);
            }
            if (maxPrice.HasValue)
            {
                where.Append(" AND p.Price <= @MaxPrice");
                parameters.Add("MaxPrice", maxPrice.Value);
            }

            var orderBy = sortBy switch
            {
                "price_asc" => "p.Price ASC",
                "price_desc" => "p.Price DESC",
                "newest" => "p.CreatedAt DESC",
                _ => "p.ProductId DESC"
            };

            parameters.Add("Offset", (page - 1) * pageSize);
            parameters.Add("PageSize", pageSize);

            var dataSql = $@"
                SELECT p.ProductId, p.Name, p.Description, p.Price, p.Stock,
                       p.ImageUrl, p.CategoryId, c.Name AS CategoryName, p.CreatedAt
                FROM Products p
                INNER JOIN Categories c ON p.CategoryId = c.CategoryId
                {where}
                ORDER BY {orderBy}
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            var countSql = $@"
                SELECT COUNT(*)
                FROM Products p
                {where}";

            using var conn = GetConnection();
            var items = (await conn.QueryAsync<ProductResponseDTO>(dataSql, parameters)).ToList();
            var totalCount = await conn.ExecuteScalarAsync<int>(countSql, parameters);

            return new PagedProductResponseDTO
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<ProductResponseDTO?> GetByIdAsync(int id)
        {
            const string sql = @"
                SELECT p.ProductId, p.Name, p.Description, p.Price, p.Stock,
                       p.ImageUrl, p.CategoryId, c.Name AS CategoryName, p.CreatedAt
                FROM Products p
                INNER JOIN Categories c ON p.CategoryId = c.CategoryId
                WHERE p.ProductId = @Id AND p.IsActive = 1";

            using var conn = GetConnection();
            return await conn.QueryFirstOrDefaultAsync<ProductResponseDTO>(sql, new { Id = id });
        }

        public async Task<List<ProductResponseDTO>> SearchAsync(string keyword)
        {
            const string sql = @"
                SELECT TOP 20 p.ProductId, p.Name, p.Description, p.Price, p.Stock,
                       p.ImageUrl, p.CategoryId, c.Name AS CategoryName, p.CreatedAt
                FROM Products p
                INNER JOIN Categories c ON p.CategoryId = c.CategoryId
                WHERE p.IsActive = 1
                  AND (p.Name LIKE @Keyword OR p.Description LIKE @Keyword)
                ORDER BY p.Name";

            using var conn = GetConnection();
            return (await conn.QueryAsync<ProductResponseDTO>(sql, new { Keyword = $"%{keyword}%" })).ToList();
        }

        public async Task<Product> CreateAsync(Product product)
        {
            _db.Products.Add(product);
            await _db.SaveChangesAsync();
            return product;
        }

        public async Task<Product> UpdateAsync(Product product)
        {
            product.UpdatedAt = DateTime.UtcNow;
            _db.Products.Update(product);
            await _db.SaveChangesAsync();
            return product;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var product = await _db.Products.FindAsync(id);
            if (product == null) return false;

            product.IsActive = false;
            product.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _db.Products.AnyAsync(p => p.ProductId == id && p.IsActive);
        }

        public async Task<Product?> GetEntityAsync(int id)
        {
            return await _db.Products.FindAsync(id);
        }
    }
}