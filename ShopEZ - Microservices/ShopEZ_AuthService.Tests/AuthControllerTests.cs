using Microsoft.AspNetCore.Mvc;
using Moq;
using ShopEZ_AuthService.Controllers;
using ShopEZ_AuthService.Services.Interfaces;
using ShopEZ_Shared_Lib.DTOs;
using ShopEZ_Shared_Lib.Helpers;
using Xunit;

namespace ShopEZ_AuthService.Tests.Controllers
{
    public class AuthControllerTests
    {
        private readonly Mock<IAuthService> _serviceMock = new();
        private AuthController CreateController() => new(_serviceMock.Object);

        //  Register 

        [Fact]
        public async Task Register_ValidInput_Returns200WithToken()
        {
            var dto = new RegisterDTO { Name = "Alice", Email = "a@b.com", Password = "Pass@1" };
            var authResponse = new AuthResponseDTO
            {
                Token = "jwt-token",
                User = new UserDTO { Id = 1, Email = "a@b.com", Role = "CUSTOMER" }
            };

            _serviceMock.Setup(s => s.RegisterAsync(dto)).ReturnsAsync(authResponse);

            var controller = CreateController();
            var result = await controller.Register(dto) as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result!.StatusCode);

            var body = result.Value as ApiResponse<AuthResponseDTO>;
            Assert.NotNull(body);
            Assert.True(body!.Success);
            Assert.Equal("jwt-token", body.Data?.Token);
        }

        [Fact]
        public async Task Register_DuplicateEmail_ServiceThrows_BubblesUp()
        {
            var dto = new RegisterDTO { Name = "Bob", Email = "dup@b.com", Password = "Pass@1" };
            _serviceMock.Setup(s => s.RegisterAsync(dto))
                        .ThrowsAsync(new InvalidOperationException("An account with this email already exists."));

            var controller = CreateController();

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => controller.Register(dto));
        }

        //  Login 

        [Fact]
        public async Task Login_ValidCredentials_Returns200()
        {
            var dto = new LoginDTO { Email = "c@d.com", Password = "Pass@1" };
            var authResponse = new AuthResponseDTO { Token = "token-123", User = new UserDTO { Id = 2 } };

            _serviceMock.Setup(s => s.LoginAsync(dto)).ReturnsAsync(authResponse);

            var result = await CreateController().Login(dto) as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result!.StatusCode);
        }

        [Fact]
        public async Task Login_BadCredentials_ServiceThrows_BubblesUp()
        {
            var dto = new LoginDTO { Email = "no@one.com", Password = "wrong" };
            _serviceMock.Setup(s => s.LoginAsync(dto))
                        .ThrowsAsync(new UnauthorizedAccessException("Invalid email or password."));

            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => CreateController().Login(dto));
        }

        //  GetAllUsers 

        [Fact]
        public async Task GetAllUsers_ReturnsListWrappedInApiResponse()
        {
            var users = new List<UserDTO>
            {
                new() { Id = 1, Name = "U1" },
                new() { Id = 2, Name = "U2" }
            };
            _serviceMock.Setup(s => s.GetAllUsersAsync()).ReturnsAsync(users);

            var result = await CreateController().GetAllUsers() as OkObjectResult;

            Assert.NotNull(result);
            var body = result!.Value as ApiResponse<List<UserDTO>>;
            Assert.Equal(2, body?.Data?.Count);
        }

        //  GetUser by id 

        [Fact]
        public async Task GetUser_ExistingId_Returns200()
        {
            _serviceMock.Setup(s => s.GetUserByIdAsync(5))
                        .ReturnsAsync(new UserDTO { Id = 5, Name = "Sam" });

            var result = await CreateController().GetUser(5) as OkObjectResult;

            Assert.Equal(200, result?.StatusCode);
        }

        [Fact]
        public async Task GetUser_NonExistingId_Returns404()
        {
            _serviceMock.Setup(s => s.GetUserByIdAsync(999))
                        .ReturnsAsync((UserDTO?)null);

            var result = await CreateController().GetUser(999);

            Assert.IsType<NotFoundObjectResult>(result);
        }
    }
}