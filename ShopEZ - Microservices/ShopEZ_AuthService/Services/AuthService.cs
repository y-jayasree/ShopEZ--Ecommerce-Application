using ShopEZ_AuthService.Models;
using ShopEZ_AuthService.Repositories.Interfaces;
using ShopEZ_AuthService.Services.Interfaces;
using ShopEZ_Shared_Lib.DTOs;
using ShopEZ_Shared_Lib.Helpers;

namespace ShopEZ_AuthService.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _users;
        private readonly IConfiguration _config;

        public AuthService(IUserRepository users, IConfiguration config)
        {
            _users = users;
            _config = config;
        }

        public async Task<AuthResponseDTO> RegisterAsync(RegisterDTO dto)
        {
            if (await _users.EmailExistsAsync(dto.Email))
                throw new InvalidOperationException("An account with this email already exists.");

            var user = new AppUser
            {
                Name = dto.Name.Trim(),
                Email = dto.Email.ToLower().Trim(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Phone = dto.Phone?.Trim(),
                Role = "CUSTOMER",
                CreatedAt = DateTime.UtcNow
            };

            var created = await _users.CreateAsync(user);
            return BuildAuthResponse(created);
        }

        public async Task<AuthResponseDTO> LoginAsync(LoginDTO dto)
        {
            var user = await _users.GetByEmailAsync(dto.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                throw new UnauthorizedAccessException("Invalid email or password.");

            if (!user.IsActive)
                throw new UnauthorizedAccessException("Your account has been deactivated.");

            return BuildAuthResponse(user);
        }

        public async Task<List<UserDTO>> GetAllUsersAsync()
        {
            var users = await _users.GetAllAsync();
            return users.Select(MapToDto).ToList();
        }

        public async Task<UserDTO?> GetUserByIdAsync(int id)
        {
            var user = await _users.GetByIdAsync(id);
            return user == null ? null : MapToDto(user);
        }

        private AuthResponseDTO BuildAuthResponse(AppUser user)
        {
            var secretKey = _config["JwtSettings:SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");
            var issuer = _config["JwtSettings:Issuer"] ?? "ShopEZ";
            var audience = _config["JwtSettings:Audience"] ?? "ShopEZ";
            var expiryMinutes = int.Parse(_config["JwtSettings:ExpiryInMinutes"] ?? "10080");

            var token = JwtHelper.GenerateToken(user.Id, user.Email, user.Role, secretKey, issuer, audience, expiryMinutes);

            return new AuthResponseDTO
            {
                Token = token,
                User = MapToDto(user)
            };
        }

        private static UserDTO MapToDto(AppUser u) => new()
        {
            Id = u.Id,
            Name = u.Name,
            Email = u.Email,
            Role = u.Role,
            Phone = u.Phone,
            CreatedAt = u.CreatedAt
        };
    }
}