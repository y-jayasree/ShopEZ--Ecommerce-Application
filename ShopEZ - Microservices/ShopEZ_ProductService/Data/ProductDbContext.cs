using Microsoft.EntityFrameworkCore;
using ShopEZ_ProductService.Models;

namespace ShopEZ_ProductService.Data
{
    public class ProductDbContext : DbContext
    {
        public ProductDbContext(DbContextOptions<ProductDbContext> options) : base(options) { }

        public DbSet<Product> Products => Set<Product>();
        public DbSet<Category> Categories => Set<Category>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>(e =>
            {
                e.HasKey(p => p.ProductId);
                e.Property(p => p.Name).IsRequired().HasMaxLength(200);
                e.Property(p => p.Price).HasColumnType("decimal(10,2)");
                e.Property(p => p.IsActive).HasDefaultValue(true);
                e.Property(p => p.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                e.HasOne(p => p.Category)
                 .WithMany(c => c.Products)
                 .HasForeignKey(p => p.CategoryId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Category>(e =>
            {
                e.HasKey(c => c.CategoryId);
                e.HasIndex(c => c.Name).IsUnique();
                e.Property(c => c.Name).IsRequired().HasMaxLength(100);
                e.Property(c => c.IsActive).HasDefaultValue(true);
                e.Property(c => c.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            });
        }
    }
}