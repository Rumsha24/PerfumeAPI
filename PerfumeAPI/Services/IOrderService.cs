using PerfumeAPI.Models.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PerfumeAPI.Services.Interfaces
{
    public interface IOrderService
    {
        Task<IEnumerable<Order>> GetUserOrdersAsync(string userId);
        Task<Order?> GetOrderByIdAsync(int id, string userId);
        Task<Order> CreateOrderAsync(string userId, string shippingAddress);
        Task<bool> CancelOrderAsync(int orderId, string userId);
        Task ProcessPaymentAsync(int orderId, PaymentRequest paymentRequest);
    }
}
