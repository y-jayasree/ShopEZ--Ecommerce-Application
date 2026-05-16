using ShopEZ_AuthService.Data;
using ShopEZ_AuthService.Models;
using Microsoft.EntityFrameworkCore;

namespace ShopEZ_AuthService.Data
{
    public static class Seeder
    {
        public static async Task SeedAdminAsync(AuthDbContext db, IConfiguration config)
        {
            var adminEmail = config["AdminSeed:Email"] ?? "admin@shopez.com";

            if (await db.Users.AnyAsync(u => u.Email == adminEmail))
                return;

            var admin = new AppUser
            {
                Name = config["AdminSeed:Name"] ?? "Super Admin",
                Email = adminEmail,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(config["AdminSeed:Password"] ?? "Admin@123"),
                Role = "ADMIN",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            db.Users.Add(admin);
            await db.SaveChangesAsync();
        }
    }
}