using Microsoft.EntityFrameworkCore;
using ShopEZ_AuthService.Models;

namespace ShopEZ_AuthService.Data
{
    public class AuthDbContext : DbContext
    {
        public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options) { }

        public DbSet<AppUser> Users => Set<AppUser>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AppUser>(e =>
            {
                e.HasKey(u => u.Id);
                e.HasIndex(u => u.Email).IsUnique();
                e.Property(u => u.Name).IsRequired().HasMaxLength(100);
                e.Property(u => u.Email).IsRequired().HasMaxLength(200);
                e.Property(u => u.PasswordHash).IsRequired();
                e.Property(u => u.Role).HasDefaultValue("CUSTOMER").HasMaxLength(20);
                e.Property(u => u.IsActive).HasDefaultValue(true);
                e.Property(u => u.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            });
        }
    }
}