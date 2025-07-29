using Microsoft.EntityFrameworkCore;
using PerfumeAPI.Data;
using PerfumeAPI.Models.Entities;
using PerfumeAPI.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PerfumeAPI.Services
{
    public class OrderService : IOrderService
    {
        private readonly AppDbContext _context;
        private readonly ICartService _cartService;
        private readonly ILogger<OrderService> _logger;

        public OrderService(
            AppDbContext context,
            ICartService cartService,
            ILogger<OrderService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _cartService = cartService ?? throw new ArgumentNullException(nameof(cartService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<Order>> GetUserOrdersAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be empty", nameof(userId));

            return await _context.Orders
                .Where(o => o.UserId == userId)
                .Include(o => o.Items)
                    .ThenInclude(oi => oi.Product)
                .Include(o => o.User)
                .OrderByDescending(o => o.OrderDate)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Order?> GetOrderByIdAsync(int id, string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be empty", nameof(userId));

            return await _context.Orders
                .Include(o => o.Items)
                    .ThenInclude(oi => oi.Product)
                .Include(o => o.User)
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);
        }

        public async Task<Order> CreateOrderAsync(string userId, string shippingAddress)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be empty", nameof(userId));

            if (string.IsNullOrWhiteSpace(shippingAddress))
                throw new ArgumentException("Shipping address cannot be empty", nameof(shippingAddress));

            var cart = await _cartService.GetUserCartAsync(userId);
            if (cart?.Items?.Any() != true)
                throw new InvalidOperationException("Cart is empty");

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Verify stock and calculate total
                var orderItems = new List<OrderItem>();
                decimal totalAmount = 0;

                foreach (var cartItem in cart.Items)
                {
                    var product = await _context.Products
                        .FirstOrDefaultAsync(p => p.Id == cartItem.ProductId);

                    if (product == null)
                        throw new InvalidOperationException($"Product {cartItem.ProductId} not found");

                    if (product.StockQuantity < cartItem.Quantity)
                        throw new InvalidOperationException(
                            $"Insufficient stock for {product.Name}. Available: {product.StockQuantity}, Requested: {cartItem.Quantity}");

                    orderItems.Add(new OrderItem
                    {
                        ProductId = product.Id,
                        Quantity = cartItem.Quantity,
                        PriceAtPurchase = product.Price
                    });

                    totalAmount += product.Price * cartItem.Quantity;
                    product.StockQuantity -= cartItem.Quantity;
                }

                var order = new Order
                {
                    UserId = userId,
                    ShippingAddress = shippingAddress,
                    Status = "Pending Payment",
                    OrderDate = DateTime.UtcNow,
                    Items = orderItems,
                    TotalAmount = totalAmount
                };
                order.GenerateOrderNumber();

                _context.Orders.Add(order);
                await _cartService.ClearCartAsync(userId);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Created new order {OrderId} for user {UserId}", order.Id, userId);
                return order;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Failed to create order for user {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> ProcessOrderPaymentAsync(int orderId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var order = await _context.Orders
                    .FirstOrDefaultAsync(o => o.Id == orderId);

                if (order == null || order.Status != "Pending Payment")
                    return false;

                // In a real implementation, integrate with payment gateway here
                order.Status = "Processing";
                order.PaymentDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Processed payment for order {OrderId}", orderId);
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Failed to process payment for order {OrderId}", orderId);
                throw;
            }
        }

        public async Task<bool> CancelOrderAsync(int orderId, string userId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var order = await _context.Orders
                    .Include(o => o.Items)
                    .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

                if (order == null ||
                    order.Status == "Cancelled" ||
                    order.Status == "Shipped" ||
                    order.Status == "Delivered")
                    return false;

                // Restore product quantities
                foreach (var item in order.Items)
                {
                    var product = await _context.Products.FindAsync(item.ProductId);
                    if (product != null)
                    {
                        product.StockQuantity += item.Quantity;
                    }
                }

                order.Status = "Cancelled";
                order.CancelledDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Cancelled order {OrderId} for user {UserId}", orderId, userId);
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Failed to cancel order {OrderId}", orderId);
                throw;
            }
        }

        public async Task<bool> ShipOrderAsync(int orderId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var order = await _context.Orders.FindAsync(orderId);
                if (order == null || order.Status != "Processing")
                    return false;

                order.Status = "Shipped";
                order.ShippedDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Shipped order {OrderId}", orderId);
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Failed to ship order {OrderId}", orderId);
                throw;
            }
        }
    }
}