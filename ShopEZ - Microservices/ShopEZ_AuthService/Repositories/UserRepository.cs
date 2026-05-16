using Microsoft.EntityFrameworkCore;
using ShopEZ_AuthService.Data;
using ShopEZ_AuthService.Models;
using ShopEZ_AuthService.Repositories.Interfaces;

namespace ShopEZ_AuthService.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AuthDbContext _db;

        public UserRepository(AuthDbContext db)
        {
            _db = db;
        }

        public async Task<AppUser?> GetByEmailAsync(string email)
        {
            return await _db.Users.FirstOrDefaultAsync(u => u.Email == email.ToLower() && u.IsActive);
        }

        public async Task<AppUser?> GetByIdAsync(int id)
        {
            return await _db.Users.FindAsync(id);
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _db.Users.AnyAsync(u => u.Email == email.ToLower());
        }

        public async Task<AppUser> CreateAsync(AppUser user)
        {
            user.Email = user.Email.ToLower().Trim();
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
            return user;
        }

        public async Task<AppUser> UpdateAsync(AppUser user)
        {
            user.UpdatedAt = DateTime.UtcNow;
            _db.Users.Update(user);
            await _db.SaveChangesAsync();
            return user;
        }

        public async Task<List<AppUser>> GetAllAsync()
        {
            return await _db.Users.OrderByDescending(u => u.CreatedAt).ToListAsync();
        }
    }
}