using System.ComponentModel.DataAnnotations;

namespace PerfumeAPI.Models.DTOs
{
    public class OrderDto
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }

        [DataType(DataType.Currency)]
        public decimal TotalAmount { get; set; }

        public List<OrderItemDto> Items { get; set; } = new List<OrderItemDto>();
        public string ShippingAddress { get; set; } = string.Empty;
    }

    public class OrderItemDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductImage { get; set; } = string.Empty;

        [Range(1, 100)]
        public int Quantity { get; set; }

        [DataType(DataType.Currency)]
        public decimal PriceAtPurchase { get; set; }

        [DataType(DataType.Currency)]
        public decimal ItemTotal => PriceAtPurchase * Quantity;
    }

    public class OrderCreateDto
    {
        [Required]
        [StringLength(200, ErrorMessage = "Address cannot exceed 200 characters")]
        public string ShippingAddress { get; set; } = string.Empty;

        [Required]
        [MinLength(1, ErrorMessage = "At least one item is required")]
        public List<OrderItemCreateDto> Items { get; set; } = new List<OrderItemCreateDto>();
    }

    public class OrderItemCreateDto
    {
        [Required]
        public int ProductId { get; set; }

        [Required]
        [Range(1, 100, ErrorMessage = "Quantity must be between 1 and 100")]
        public int Quantity { get; set; }
    }
}