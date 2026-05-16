using Microsoft.EntityFrameworkCore;
using ShopEZ_OrderService.Models;

namespace ShopEZ_OrderService.Data
{
    public class OrderDbContext : DbContext
    {
        public OrderDbContext(DbContextOptions<OrderDbContext> options) : base(options) { }

        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Order>(e =>
            {
                e.HasKey(o => o.OrderId);
                e.HasIndex(o => o.OrderNumber).IsUnique();
                e.Property(o => o.TotalAmount).HasColumnType("decimal(10,2)");
                e.Property(o => o.Status).HasDefaultValue("PENDING").HasMaxLength(50);
                e.Property(o => o.OrderDate).HasDefaultValueSql("GETUTCDATE()");
                e.HasMany(o => o.Items)
                 .WithOne(i => i.Order)
                 .HasForeignKey(i => i.OrderId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<OrderItem>(e =>
            {
                e.HasKey(i => i.OrderItemId);
                e.Property(i => i.UnitPrice).HasColumnType("decimal(10,2)");
            });
        }
    }
}