using System;
using System.Threading.Tasks;

namespace PerfumeAPI.Services
{
    public class PaymentService : IPaymentService
    {
        public async Task<PaymentResult> ProcessPaymentAsync(PaymentRequest request)
        {
            // Simulate async payment processing
            await Task.Delay(500);

            return new PaymentResult
            {
                Success = true,
                TransactionId = Guid.NewGuid().ToString(),
                ErrorMessage = null
            };
        }
    }
}
