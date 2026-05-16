using Microsoft.EntityFrameworkCore;
using ShopEZ_cartservice.Models;

namespace ShopEZ_cartservice.Data
{
    public class CartDbContext : DbContext
    {
        public CartDbContext(DbContextOptions<CartDbContext> options) : base(options) { }

        public DbSet<CartItem> CartItems => Set<CartItem>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CartItem>(e =>
            {
                e.HasKey(c => c.Id);
                e.HasIndex(c => new { c.UserId, c.ProductId }).IsUnique();
                e.Property(c => c.UnitPrice).HasColumnType("decimal(10,2)");
                e.Property(c => c.AddedAt).HasDefaultValueSql("GETUTCDATE()");
            });
        }
    }
}
