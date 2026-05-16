using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopEZ_cartservice.Services.Interfaces;
using ShopEZ_Shared_Lib.DTOs;
using ShopEZ_Shared_Lib.Helpers;

namespace ShopEZ_cartservice.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/cart")]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        [HttpGet]
        public async Task<IActionResult> GetCart()
        {
            var userId = JwtHelper.GetUserId(User);
            if (userId == 0)
                return Unauthorized(ApiResponse<object>.Fail("Invalid token"));

            var cart = await _cartService.GetCartAsync(userId);
            return Ok(ApiResponse<CartResponseDTO>.Ok(cart));
        }

        [HttpPost("items")]
        public async Task<IActionResult> AddItem([FromBody] AddToCartDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Fail(GetModelErrors()));

            var userId = JwtHelper.GetUserId(User);
            if (userId == 0)
                return Unauthorized(ApiResponse<object>.Fail("Invalid token"));

            var cart = await _cartService.AddItemAsync(userId, dto);
            return Ok(ApiResponse<CartResponseDTO>.Ok(cart, "Item added to cart"));
        }

        [HttpPut("items/{productId:int}")]
        public async Task<IActionResult> UpdateItem(int productId, [FromBody] UpdateCartItemDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Fail(GetModelErrors()));

            var userId = JwtHelper.GetUserId(User);
            if (userId == 0)
                return Unauthorized(ApiResponse<object>.Fail("Invalid token"));

            var cart = await _cartService.UpdateItemAsync(userId, productId, dto);
            return Ok(ApiResponse<CartResponseDTO>.Ok(cart, "Cart updated"));
        }

        [HttpDelete("items/{productId:int}")]
        public async Task<IActionResult> RemoveItem(int productId)
        {
            var userId = JwtHelper.GetUserId(User);
            if (userId == 0)
                return Unauthorized(ApiResponse<object>.Fail("Invalid token"));

            var cart = await _cartService.RemoveItemAsync(userId, productId);
            return Ok(ApiResponse<CartResponseDTO>.Ok(cart, "Item removed"));
        }

        [HttpDelete]
        public async Task<IActionResult> ClearCart()
        {
            var userId = JwtHelper.GetUserId(User);
            if (userId == 0)
                return Unauthorized(ApiResponse<object>.Fail("Invalid token"));

            var cart = await _cartService.ClearCartAsync(userId);
            return Ok(ApiResponse<CartResponseDTO>.Ok(cart, "Cart cleared"));
        }

        private string GetModelErrors()
        {
            return string.Join("; ", ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage));
        }
    }
}