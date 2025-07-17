using System;

namespace PerfumeAPI.Models.DTOs
{
    public class CartItemDto
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public string ProductName { get; set; }
        public string ProductImage { get; set; }
        public decimal Price { get; set; }
        public decimal ItemTotal => Price * Quantity;
        public DateTime AddedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string FragranceType { get; set; }
    }
}