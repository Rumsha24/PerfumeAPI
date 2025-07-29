using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PerfumeAPI.Models.Entities;
using PerfumeAPI.Services.Interfaces;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PerfumeAPI.Controllers
{
    [Authorize]
    [Route("orders")]
    public class OrderController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly ILogger<OrderController> _logger;

        public OrderController(
            IOrderService orderService,
            ILogger<OrderController> logger)
        {
            _orderService = orderService;
            _logger = logger;
        }

        // GET: orders/
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("Unauthorized access attempt to orders");
                return Challenge();
            }

            try
            {
                var orders = await _orderService.GetUserOrdersAsync(userId);
                return View(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving orders for user {UserId}", userId);
                TempData["Error"] = "An error occurred while retrieving your orders";
                return RedirectToAction("Index", "Home");
            }
        }

        // GET: orders/details/5
        [HttpGet("details/{id:int}")]
        public async Task<IActionResult> Details(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("Unauthorized access attempt to order details");
                return Challenge();
            }

            try
            {
                var order = await _orderService.GetOrderByIdAsync(id, userId);
                if (order == null)
                {
                    _logger.LogWarning("Order not found: {OrderId} for user {UserId}", id, userId);
                    return NotFound();
                }
                return View("Details", order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving order {OrderId} for user {UserId}", id, userId);
                TempData["Error"] = "An error occurred while retrieving order details";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: orders/checkout
        [HttpPost("checkout")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(string shippingAddress)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("Unauthorized checkout attempt");
                return Challenge();
            }

            try
            {
                var order = await _orderService.CreateOrderAsync(userId, shippingAddress);
                return RedirectToAction("OrderConfirmation", new { id = order.Id });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Checkout failed for user {UserId}", userId);
                TempData["Error"] = ex.Message;
                return RedirectToAction("Index", "Cart");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during checkout for user {UserId}", userId);
                TempData["Error"] = "An error occurred while processing your order";
                return RedirectToAction("Index", "Cart");
            }
        }

        // GET: orders/confirmation/5
        [HttpGet("confirmation/{id:int}")]
        public async Task<IActionResult> OrderConfirmation(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("Unauthorized access to order confirmation");
                return Challenge();
            }

            try
            {
                var order = await _orderService.GetOrderByIdAsync(id, userId);
                if (order == null)
                {
                    _logger.LogWarning("Order confirmation not found: {OrderId} for user {UserId}", id, userId);
                    return NotFound();
                }
                return View(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving order confirmation {OrderId} for user {UserId}", id, userId);
                TempData["Error"] = "An error occurred while loading your order confirmation";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: orders/cancel/5
        [HttpPost("cancel/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelOrder(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("Unauthorized order cancellation attempt");
                return Challenge();
            }

            try
            {
                var success = await _orderService.CancelOrderAsync(id, userId);
                if (!success)
                {
                    TempData["Error"] = "Order could not be cancelled";
                    return RedirectToAction(nameof(Details), new { id });
                }

                TempData["Success"] = "Order has been cancelled successfully";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling order {OrderId} for user {UserId}", id, userId);
                TempData["Error"] = "An error occurred while cancelling your order";
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        // POST: orders/payment/process/5
        [HttpPost("payment/process/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessPayment(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("Unauthorized payment processing attempt");
                return Challenge();
            }

            try
            {
                var success = await _orderService.ProcessOrderPaymentAsync(id);
                if (!success)
                {
                    TempData["Error"] = "Payment could not be processed";
                    return RedirectToAction(nameof(Details), new { id });
                }

                TempData["Success"] = "Payment processed successfully";
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment for order {OrderId} by user {UserId}", id, userId);
                TempData["Error"] = "An error occurred while processing your payment";
                return RedirectToAction(nameof(Details), new { id });
            }
        }
    }
}