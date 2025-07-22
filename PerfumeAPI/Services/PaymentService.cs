using System;
using System.Threading.Tasks;
using PerfumeAPI.Services.Interfaces;

namespace PerfumeAPI.Services
{
    public class PaymentService : IPaymentService
    {
        public async Task<PaymentResult> ProcessPaymentAsync(PaymentRequest request)
        {
            // Dummy implementation — replace with real payment logic
            await Task.Delay(500);

            return new PaymentResult
            {
                Success = true,
                TransactionId = Guid.NewGuid().ToString(),
                ErrorMessage = string.Empty // Changed from null to empty string
            };
        }
    }
}