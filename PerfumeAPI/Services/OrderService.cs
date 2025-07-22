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
        private readonly IPaymentService _paymentService;

        public OrderService(
            AppDbContext context,
            ICartService cartService,
            IPaymentService paymentService)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _cartService = cartService ?? throw new ArgumentNullException(nameof(cartService));
            _paymentService = paymentService ?? throw new ArgumentNullException(nameof(paymentService));
        }

        public async Task<IEnumerable<Order>> GetUserOrdersAsync(string userId)
        {
            return await _context.Orders
                .Where(o => o.UserId == userId)
                .Include(o => o.Items)
                .ThenInclude(oi => oi.Product)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task<Order?> GetOrderByIdAsync(int id, string userId)
        {
            return await _context.Orders
                .Include(o => o.Items)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);
        }

        public async Task<Order> CreateOrderAsync(string userId, string shippingAddress)
        {
            var cart = await _cartService.GetUserCartAsync(userId);
            if (cart?.Items?.Any() != true)
                throw new InvalidOperationException("Cart is empty");

            var order = new Order
            {
                UserId = userId,
                ShippingAddress = shippingAddress,
                Status = "Processing",
                OrderDate = DateTime.UtcNow,
                Items = cart.Items.Select(i => new OrderItem
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    PriceAtPurchase = i.Product?.Price ?? 0m
                }).ToList(),
                TotalAmount = cart.Items.Sum(i => (i.Product?.Price ?? 0m) * i.Quantity +
                              (i.Product?.ShippingCost ?? 0m))
            };

            _context.Orders.Add(order);
            await _cartService.ClearCartAsync(userId);
            await _context.SaveChangesAsync();

            return order;
        }

        public async Task<bool> CancelOrderAsync(int orderId, string userId)
        {
            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

            if (order == null || order.Status != "Processing")
                return false;

            order.Status = "Cancelled";
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task ProcessPaymentAsync(int orderId, PaymentRequest paymentRequest)
        {
            var order = await _context.Orders.FindAsync(orderId)
                ?? throw new ArgumentException("Order not found");

            paymentRequest.Amount = order.TotalAmount;
            paymentRequest.Description = $"Payment for order #{order.Id}";

            var result = await _paymentService.ProcessPaymentAsync(paymentRequest);

            order.Status = result.Success ? "Payment Received" : "Payment Failed";
            await _context.SaveChangesAsync();

            if (!result.Success)
                throw new Exception(result.ErrorMessage ?? "Payment processing failed");
        }
    }
}