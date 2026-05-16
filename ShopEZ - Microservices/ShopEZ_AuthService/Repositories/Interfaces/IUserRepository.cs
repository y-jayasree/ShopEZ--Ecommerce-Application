using ShopEZ_AuthService.Models;

namespace ShopEZ_AuthService.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<AppUser?> GetByEmailAsync(string email);
        Task<AppUser?> GetByIdAsync(int id);
        Task<bool> EmailExistsAsync(string email);
        Task<AppUser> CreateAsync(AppUser user);
        Task<AppUser> UpdateAsync(AppUser user);
        Task<List<AppUser>> GetAllAsync();
    }
}