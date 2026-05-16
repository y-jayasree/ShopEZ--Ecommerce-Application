using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopEZ_AuthService.Services.Interfaces;
using ShopEZ_Shared_Lib.DTOs;
using ShopEZ_Shared_Lib.Helpers;

namespace ShopEZ_AuthService.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Fail(GetModelErrors()));

            var result = await _authService.RegisterAsync(dto);
            return Ok(ApiResponse<AuthResponseDTO>.Ok(result, "Registration successful"));
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Fail(GetModelErrors()));

            var result = await _authService.LoginAsync(dto);
            return Ok(ApiResponse<AuthResponseDTO>.Ok(result, "Login successful"));
        }

        //[Authorize]
        //[HttpGet("me")]
        //public async Task<IActionResult> GetCurrentUser()
        //{
        //    var userId = JwtHelper.GetUserId(User);
        //    var user = await _authService.GetUserByIdAsync(userId);

        //    if (user == null)
        //        return NotFound(ApiResponse<object>.Fail("User not found"));

        //    return Ok(ApiResponse<UserDTO>.Ok(user));
        //}

        //[Authorize]
        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _authService.GetAllUsersAsync();
            return Ok(ApiResponse<List<UserDTO>>.Ok(users, "Users retrieved", users.Count));
        }

        [Authorize(Roles = "ADMIN")]
        [HttpGet("users/{id:int}")]
        public async Task<IActionResult> GetUser(int id)
        {
            var user = await _authService.GetUserByIdAsync(id);

            if (user == null)
                return NotFound(ApiResponse<object>.Fail("User not found"));

            return Ok(ApiResponse<UserDTO>.Ok(user));
        }

        private string GetModelErrors()
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage);
            return string.Join("; ", errors);
        }
    }
}