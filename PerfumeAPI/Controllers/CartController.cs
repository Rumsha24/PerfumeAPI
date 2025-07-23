using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PerfumeAPI.Models.DTOs;
using PerfumeAPI.Services;
using PerfumeAPI.Services.Interfaces;
using System.Security.Claims;

namespace PerfumeAPI.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly ICartService _cartService;
        private readonly ILogger<CartController> _logger;

        public CartController(ICartService cartService, ILogger<CartController> logger)
        {
            _cartService = cartService ?? throw new ArgumentNullException(nameof(cartService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var userId = GetUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                var cartDto = await _cartService.GetCartDtoAsync(userId);
                return View(cartDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading cart for user {UserId}", GetUserId());
                return StatusCode(500, "An error occurred while loading your cart");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            try
            {
                var userId = GetUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                if (quantity < 1)
                {
                    TempData["Error"] = "Quantity must be at least 1";
                    return RedirectToAction("Details", "Product", new { id = productId });
                }

                await _cartService.AddToCartAsync(userId, productId, quantity);
                TempData["Success"] = "Product added to cart successfully";
                return RedirectToAction(nameof(Index));
            }
            catch (InsufficientStockException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Details", "Product", new { id = productId });
            }
            catch (ProductNotFoundException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Index", "Product");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding product {ProductId} to cart for user {UserId}",
                    productId, GetUserId());
                TempData["Error"] = "Failed to add product to cart";
                return RedirectToAction("Details", "Product", new { id = productId });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateQuantity(int productId, int quantity)
        {
            try
            {
                var userId = GetUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                if (quantity < 1)
                {
                    TempData["Error"] = "Quantity must be at least 1";
                    return RedirectToAction(nameof(Index));
                }

                await _cartService.UpdateQuantityAsync(userId, productId, quantity);
                TempData["Success"] = "Quantity updated successfully";
                return RedirectToAction(nameof(Index));
            }
            catch (CartItemNotFoundException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
            catch (InsufficientStockException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating quantity for product {ProductId} for user {UserId}",
                    productId, GetUserId());
                TempData["Error"] = "Failed to update quantity";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveItem(int productId)
        {
            try
            {
                var userId = GetUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                await _cartService.RemoveFromCartAsync(userId, productId);
                TempData["Success"] = "Item removed from cart successfully";
                return RedirectToAction(nameof(Index));
            }
            catch (CartItemNotFoundException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing product {ProductId} from cart for user {UserId}",
                    productId, GetUserId());
                TempData["Error"] = "Failed to remove item from cart";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ClearCart()
        {
            try
            {
                var userId = GetUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                await _cartService.ClearCartAsync(userId);
                TempData["Success"] = "Cart cleared successfully";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing cart for user {UserId}", GetUserId());
                TempData["Error"] = "Failed to clear cart";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetCartCount()
        {
            try
            {
                var userId = GetUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new { count = 0 });
                }

                var count = await _cartService.GetCartItemCountAsync(userId);
                return Json(new { count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cart count for user {UserId}", GetUserId());
                return Json(new { count = 0 });
            }
        }

        private string? GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier);
        }
    }
}