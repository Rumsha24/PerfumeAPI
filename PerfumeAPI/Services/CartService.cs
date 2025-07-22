using Microsoft.EntityFrameworkCore;
using PerfumeAPI.Data;
using PerfumeAPI.Models.DTOs;
using PerfumeAPI.Models.Entities;
using PerfumeAPI.Services.Interfaces;
using System.Security.Claims;

namespace PerfumeAPI.Services
{
    public class CartService : ICartService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<CartService> _logger;

        public CartService(AppDbContext context, ILogger<CartService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Cart> GetUserCartAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be empty", nameof(userId));

            try
            {
                var cart = await _context.Carts
                    .Include(c => c.Items)
                    .ThenInclude(i => i.Product)
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                return cart ?? await CreateCartAsync(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving cart for user {UserId}", userId);
                throw new CartServiceException("Could not retrieve user cart", ex);
            }
        }

        public async Task AddToCartAsync(string userId, int productId, int quantity = 1)
        {
            if (quantity < 1)
                throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be at least 1");

            try
            {
                var cart = await GetUserCartAsync(userId);
                var product = await _context.Products.FindAsync(productId)
                    ?? throw new ProductNotFoundException(productId);

                var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == productId);

                if (existingItem != null)
                {
                    existingItem.Quantity += quantity;
                    existingItem.UpdatedAt = DateTime.UtcNow;
                }
                else
                {
                    cart.Items.Add(new CartItem
                    {
                        ProductId = productId,
                        Quantity = quantity,
                        AddedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    });
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("Added product {ProductId} to cart for user {UserId}", productId, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding product {ProductId} to cart for user {UserId}", productId, userId);
                throw;
            }
        }

        public async Task RemoveFromCartAsync(string userId, int productId)
        {
            try
            {
                var cart = await GetUserCartAsync(userId);
                var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);

                if (item != null)
                {
                    _context.CartItems.Remove(item);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Removed product {ProductId} from cart for user {UserId}", productId, userId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing product {ProductId} from cart for user {UserId}", productId, userId);
                throw;
            }
        }

        public async Task UpdateQuantityAsync(string userId, int productId, int quantity)
        {
            if (quantity < 1)
                throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be at least 1");

            try
            {
                var cart = await GetUserCartAsync(userId);
                var item = cart.Items.FirstOrDefault(i => i.ProductId == productId)
                    ?? throw new CartItemNotFoundException(productId);

                item.Quantity = quantity;
                item.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                _logger.LogInformation("Updated quantity for product {ProductId} in cart for user {UserId}", productId, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating quantity for product {ProductId} in cart for user {UserId}", productId, userId);
                throw;
            }
        }

        public async Task ClearCartAsync(string userId)
        {
            try
            {
                var cart = await GetUserCartAsync(userId);
                _context.CartItems.RemoveRange(cart.Items);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Cleared cart for user {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing cart for user {UserId}", userId);
                throw;
            }
        }

        public async Task<int> GetCartItemCountAsync(string userId)
        {
            try
            {
                var cart = await GetUserCartAsync(userId);
                return cart.Items.Sum(i => i.Quantity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cart item count for user {UserId}", userId);
                throw;
            }
        }

        public async Task<CartDto> GetCartDtoAsync(string userId)
        {
            try
            {
                var cart = await GetUserCartAsync(userId);
                return new CartDto
                {
                    Id = cart.Id,
                    UserId = cart.UserId,
                    Items = cart.Items.Select(i => new CartItemDto
                    {
                        ProductId = i.ProductId,
                        Quantity = i.Quantity,
                        ProductName = i.Product?.Name,
                        ProductImage = i.Product?.ImageUrl,
                        Price = i.Product?.Price ?? 0,
                        AddedAt = i.AddedAt,
                        UpdatedAt = i.UpdatedAt
                    }).ToList(),
                    Subtotal = cart.Items.Sum(i => (i.Product?.Price ?? 0) * i.Quantity),
                    ShippingTotal = cart.Items.Sum(i => i.Product?.ShippingCost ?? 0),
                    GrandTotal = cart.Items.Sum(i => (i.Product?.Price ?? 0) * i.Quantity) +
                                cart.Items.Sum(i => i.Product?.ShippingCost ?? 0)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cart DTO for user {UserId}", userId);
                throw;
            }
        }

        private async Task<Cart> CreateCartAsync(string userId)
        {
            var cart = new Cart { UserId = userId };
            _context.Carts.Add(cart);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Created new cart for user {UserId}", userId);
            return cart;
        }
    }

    public class CartServiceException : Exception
    {
        public CartServiceException(string message, Exception innerException)
            : base(message, innerException) { }
    }

    public class ProductNotFoundException : Exception
    {
        public ProductNotFoundException(int productId)
            : base($"Product with ID {productId} not found") { }
    }

    public class CartItemNotFoundException : Exception
    {
        public CartItemNotFoundException(int productId)
            : base($"Cart item for product ID {productId} not found") { }
    }
}