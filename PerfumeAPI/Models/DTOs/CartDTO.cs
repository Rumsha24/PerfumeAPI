using System.ComponentModel.DataAnnotations;

namespace PerfumeAPI.Models.DTOs
{
    public class CartDto
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public List<CartItemDto> Items { get; set; } = new List<CartItemDto>();

        [DataType(DataType.Currency)]
        public decimal Subtotal { get; set; }

        [DataType(DataType.Currency)]
        public decimal ShippingTotal { get; set; }

        [DataType(DataType.Currency)]
        public decimal GrandTotal { get; set; }

        public int TotalItems => Items.Sum(i => i.Quantity);
    }
}