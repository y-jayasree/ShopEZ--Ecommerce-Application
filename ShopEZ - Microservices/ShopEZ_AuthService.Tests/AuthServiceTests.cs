using Microsoft.Extensions.Configuration;
using Moq;
using ShopEZ_AuthService.Models;
using ShopEZ_AuthService.Repositories.Interfaces;
using ShopEZ_AuthService.Services;
using ShopEZ_Shared_Lib.DTOs;
using Xunit;

namespace ShopEZ_AuthService.Tests.Services
{
    public class AuthServiceTests
    {
        // Helpers / shared setup
        private readonly Mock<IUserRepository> _repoMock = new();
        private readonly IConfiguration _config;

        public AuthServiceTests()
        {
            // Minimal in-memory config so JwtHelper.GenerateToken never throws
            var configData = new Dictionary<string, string?>
            {
                ["JwtSettings:SecretKey"] = "TestSecretKey_AtLeast32CharactersLong!",
                ["JwtSettings:Issuer"] = "ShopEZ-Test",
                ["JwtSettings:Audience"] = "ShopEZ-Test",
                ["JwtSettings:ExpiryInMinutes"] = "60"
            };
            _config = new ConfigurationBuilder()
                .AddInMemoryCollection(configData)
                .Build();
        }

        private AuthService CreateService() => new(_repoMock.Object, _config);
  
        // RegisterAsync
        [Fact]
        public async Task Register_WithValidData_ReturnsTokenAndUser()
        {
            // Arrange
            var dto = new RegisterDTO
            {
                Name = "Alice",
                Email = "alice@example.com",
                Password = "Pass@1234",
                Phone = "9876543210"
            };

            _repoMock.Setup(r => r.EmailExistsAsync(dto.Email))
                     .ReturnsAsync(false);

            _repoMock.Setup(r => r.CreateAsync(It.IsAny<AppUser>()))
                     .ReturnsAsync((AppUser u) =>
                     {
                         u.Id = 1;
                         return u;
                     });

            var result = await CreateService().RegisterAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.False(string.IsNullOrWhiteSpace(result.Token), "JWT token must not be empty");
            Assert.Equal("alice@example.com", result.User.Email);
            Assert.Equal("CUSTOMER", result.User.Role);
        }

        [Fact]
        public async Task Register_EmailAlreadyExists_ThrowsInvalidOperationException()
        {
            // Arrange
            var dto = new RegisterDTO
            {
                Name = "Bob",
                Email = "bob@example.com",
                Password = "Pass@1234"
            };

            _repoMock.Setup(r => r.EmailExistsAsync(dto.Email))
                     .ReturnsAsync(true);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(
                () => CreateService().RegisterAsync(dto));

            Assert.Contains("already exists", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task Register_TrimsNameAndLowerCasesEmail()
        {
            // Arrange
            var dto = new RegisterDTO
            {
                Name = "  Charlie  ",
                Email = "  CHARLIE@EXAMPLE.COM  ",
                Password = "Pass@1234"
            };

            AppUser? captured = null;

            _repoMock.Setup(r => r.EmailExistsAsync(It.IsAny<string>()))
                     .ReturnsAsync(false);

            _repoMock.Setup(r => r.CreateAsync(It.IsAny<AppUser>()))
                     .Callback<AppUser>(u => captured = u)
                     .ReturnsAsync((AppUser u) => { u.Id = 2; return u; });

            // Act
            await CreateService().RegisterAsync(dto);
            Assert.NotNull(captured);
            Assert.Equal("Charlie", captured!.Name);
            Assert.Equal("charlie@example.com", captured.Email);
        }

        // LoginAsync

        [Fact]
        public async Task Login_WithCorrectCredentials_ReturnsToken()
        {
            // Arrange
            var plainPassword = "Pass@1234";
            var user = new AppUser
            {
                Id = 1,
                Name = "Dave",
                Email = "dave@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(plainPassword),
                Role = "CUSTOMER",
                IsActive = true
            };

            var dto = new LoginDTO { Email = "dave@example.com", Password = plainPassword };

            _repoMock.Setup(r => r.GetByEmailAsync(dto.Email))
                     .ReturnsAsync(user);

            // Act
            var result = await CreateService().LoginAsync(dto);

            // Assert
            Assert.False(string.IsNullOrWhiteSpace(result.Token));
            Assert.Equal(user.Email, result.User.Email);
        }

        [Fact]
        public async Task Login_WithWrongPassword_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var user = new AppUser
            {
                Id = 1,
                Email = "eve@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("CorrectPass@1"),
                IsActive = true
            };

            var dto = new LoginDTO { Email = "eve@example.com", Password = "WrongPass@1" };

            _repoMock.Setup(r => r.GetByEmailAsync(dto.Email))
                     .ReturnsAsync(user);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => CreateService().LoginAsync(dto));
        }

        [Fact]
        public async Task Login_WithNonExistentEmail_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            _repoMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>()))
                     .ReturnsAsync((AppUser?)null);

            var dto = new LoginDTO { Email = "ghost@example.com", Password = "Pass@1234" };

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => CreateService().LoginAsync(dto));
        }

        [Fact]
        public async Task Login_WithDeactivatedAccount_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var plainPassword = "Pass@1234";
            var user = new AppUser
            {
                Id = 3,
                Email = "frank@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(plainPassword),
                IsActive = false   
            };

            var dto = new LoginDTO { Email = "frank@example.com", Password = plainPassword };

            _repoMock.Setup(r => r.GetByEmailAsync(dto.Email))
                     .ReturnsAsync(user);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => CreateService().LoginAsync(dto));

            Assert.Contains("deactivated", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        // GetUserByIdAsync

        [Fact]
        public async Task GetUserById_ExistingId_ReturnsUserDTO()
        {
            var user = new AppUser { Id = 10, Name = "Grace", Email = "grace@example.com", Role = "CUSTOMER" };
            _repoMock.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(user);

            var result = await CreateService().GetUserByIdAsync(10);

            Assert.NotNull(result);
            Assert.Equal("Grace", result!.Name);
            Assert.Equal("grace@example.com", result.Email);
        }

        [Fact]
        public async Task GetUserById_NonExistingId_ReturnsNull()
        {
            _repoMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((AppUser?)null);

            var result = await CreateService().GetUserByIdAsync(999);

            Assert.Null(result);
        }

        // GetAllUsersAsync

        [Fact]
        public async Task GetAllUsers_ReturnsMappedList()
        {
            var users = new List<AppUser>
            {
                new() { Id = 1, Name = "User1", Email = "u1@example.com", Role = "CUSTOMER" },
                new() { Id = 2, Name = "User2", Email = "u2@example.com", Role = "ADMIN" }
            };
            _repoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(users);

            var result = await CreateService().GetAllUsersAsync();

            Assert.Equal(2, result.Count);
            Assert.Equal("User1", result[0].Name);
            Assert.Equal("ADMIN", result[1].Role);
        }

        [Fact]
        public async Task GetAllUsers_EmptyRepository_ReturnsEmptyList()
        {
            _repoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<AppUser>());

            var result = await CreateService().GetAllUsersAsync();

            Assert.Empty(result);
        }
    }
}