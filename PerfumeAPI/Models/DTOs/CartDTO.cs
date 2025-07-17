using System.Collections.Generic;

namespace PerfumeAPI.Models.DTOs
{
    public class CartDto
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public List<CartItemDto> Items { get; set; } // Ensure this matches the type
        public decimal Subtotal { get; set; }
        public decimal ShippingTotal { get; set; }
        public decimal GrandTotal { get; set; }
    }
}