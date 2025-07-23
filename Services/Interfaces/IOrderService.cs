using PerfumeAPI.Models.Entities;
using PerfumeAPI.Services;

public interface IOrderService
{
    Task<IEnumerable<Order>> GetUserOrdersAsync(string userId);
    Task<Order?> GetOrderByIdAsync(int id, string userId); // Changed to nullable
    Task<Order> CreateOrderAsync(string userId, string shippingAddress);
    Task<bool> CancelOrderAsync(int orderId, string userId);
    Task ProcessPaymentAsync(int orderId, PaymentRequest paymentRequest);
}