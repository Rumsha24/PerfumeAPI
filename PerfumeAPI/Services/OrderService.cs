using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.EntityFrameworkCore;
using PerfumeAPI.Data;
using PerfumeAPI.Models.Entities;
using PerfumeAPI.Services.Interfaces;

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
            _context = context;
            _cartService = cartService;
            _paymentService = paymentService;
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

        public async Task<Order> GetOrderByIdAsync(int id, string userId)
        {
            return await _context.Orders
                .Include(o => o.Items)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);
        }

        public async Task<Order> CreateOrderAsync(string userId, string shippingAddress)
        {
            var cart = await _cartService.GetUserCartAsync(userId);
            if (!cart.Items.Any())
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
                    PriceAtPurchase = i.Product.Price
                }).ToList(),
                TotalAmount = cart.Items.Sum(i => i.Product.Price * i.Quantity) +
                             cart.Items.Sum(i => i.Product.ShippingCost)
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
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
                throw new ArgumentException("Order not found");

            paymentRequest.Amount = order.TotalAmount;
            paymentRequest.Description = $"Payment for order #{order.OrderNumber}";

            var result = await _paymentService.ProcessPaymentAsync(paymentRequest);

            if (result.Success)
            {
                order.Status = "Payment Received";
            }
            else
            {
                order.Status = "Payment Failed";
                throw new Exception(result.ErrorMessage);
            }

            await _context.SaveChangesAsync();
        }
    }
}