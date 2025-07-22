namespace PerfumeAPI.Services
{
    public class PaymentService : IPaymentService
    {
        public async Task<PaymentResult> ProcessPaymentAsync(PaymentRequest request)
        {
            // Simulate payment processing (for development/testing)
            await Task.Delay(500); // simulate async delay

            return new PaymentResult
            {
                Success = true,
                TransactionId = Guid.NewGuid().ToString(),
                ErrorMessage = null
            };
        }
    }
}
