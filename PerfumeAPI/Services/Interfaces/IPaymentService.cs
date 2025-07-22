using System.Threading.Tasks;

namespace PerfumeAPI.Services.Interfaces
{
    public interface IPaymentService
    {
        Task<PaymentResult> ProcessPaymentAsync(PaymentRequest request);
    }

    public class PaymentRequest
    {
        public required string CardNumber { get; set; }
        public required string ExpiryDate { get; set; }
        public required string CVV { get; set; }
        public decimal Amount { get; set; }
        public required string Description { get; set; }
    }

    public class PaymentResult
    {
        public bool Success { get; set; }
        public string TransactionId { get; set; } = string.Empty;
        public string? ErrorMessage { get; set; } // Made nullable
    }
}