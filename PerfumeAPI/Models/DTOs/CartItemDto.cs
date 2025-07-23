using System.ComponentModel.DataAnnotations;

namespace PerfumeAPI.Models.DTOs
{
    public class CartItemDto
    {
        public int Id { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required]
        [Range(1, 100, ErrorMessage = "Quantity must be between 1 and 100")]
        public int Quantity { get; set; }

        public string ProductName { get; set; } = string.Empty;
        public string ProductImage { get; set; } = string.Empty;

        [DataType(DataType.Currency)]
        public decimal Price { get; set; }

        [DataType(DataType.Currency)]
        public decimal ItemTotal => Price * Quantity;

        public DateTime AddedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string FragranceType { get; set; } = string.Empty;
    }
}