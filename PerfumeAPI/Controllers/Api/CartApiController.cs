using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PerfumeAPI.Models.DTOs;
using PerfumeAPI.Services.Interfaces;
using System.Security.Claims;

namespace PerfumeAPI.Controllers.Api
{
    [Route("api/cart")]
    [ApiController]
    [Authorize]
    public class CartApiController : ControllerBase
    {
        private readonly ICartService _cartService;

        public CartApiController(ICartService cartService)
        {
            _cartService = cartService;
        }

        [HttpGet]
        public async Task<ActionResult<CartDto>> GetCart()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }
            return await _cartService.GetCartDtoAsync(userId);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart([FromBody] CartItemDto itemDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }
            await _cartService.AddToCartAsync(userId, itemDto.ProductId, itemDto.Quantity);
            return NoContent();
        }

        [HttpDelete("{productId}")]
        public async Task<IActionResult> RemoveFromCart(int productId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }
            await _cartService.RemoveFromCartAsync(userId, productId);
            return NoContent();
        }
    }
}