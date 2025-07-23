using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PerfumeAPI.Data;
using PerfumeAPI.Models.Entities;
using System.Security.Claims;

namespace PerfumeAPI.Controllers.Api
{
    [Route("api/orders")]
    [ApiController]
    [Authorize]
    public class OrderApiController : ControllerBase
    {
        private readonly AppDbContext _context;

        public OrderApiController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrders()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized();
            }

            return await _context.Orders
                .Where(o => o.UserId == userId)
                .Include(o => o.Items)
                .ThenInclude(oi => oi.Product)
                .AsNoTracking()
                .ToListAsync();
        }

        [HttpPost]
        public async Task<ActionResult<Order>> CreateOrder([FromBody] OrderCreateDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.ShippingAddress))
            {
                return BadRequest("Shipping address is required");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized();
            }

            var cart = await _context.Carts
                .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null || !cart.Items.Any())
            {
                return BadRequest("Cart is empty");
            }

            var order = new Order
            {
                UserId = userId,
                ShippingAddress = dto.ShippingAddress,
                Status = "Processing",
                OrderDate = DateTime.UtcNow,
                OrderNumber = Guid.NewGuid().ToString()[..8].ToUpper(),
                Items = cart.Items.Select(i => new OrderItem
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    PriceAtPurchase = i.Product.Price
                }).ToList(),
                TotalAmount = cart.Items.Sum(i => i.Product.Price * i.Quantity)
            };

            _context.Orders.Add(order);
            _context.CartItems.RemoveRange(cart.Items);

            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetOrders), new { id = order.Id }, order);
        }
    }

    public class OrderCreateDto
    {
        public string ShippingAddress { get; set; } = string.Empty;
    }
}