using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace PerfumeAPI.Models.DTOs
{
    public class ProductUpdateDto
    {
        public int Id { get; set; }  // Add this line

        [Required]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Range(0.01, 10000, ErrorMessage = "Price must be between 0.01 and 10,000")]
        public decimal Price { get; set; }

        [Required]
        [Range(0, 1000, ErrorMessage = "Shipping cost must be between 0 and 1,000")]
        public decimal ShippingCost { get; set; }

        [Required]
        [StringLength(50, ErrorMessage = "Fragrance type cannot exceed 50 characters")]
        public string FragranceType { get; set; } = string.Empty;

        [StringLength(50)]
        public string? FragranceFamily { get; set; }

        [Required]
        [StringLength(20, ErrorMessage = "Size cannot exceed 20 characters")]
        public string Size { get; set; } = string.Empty;

        // Inventory Properties for Update
        [Range(0, int.MaxValue, ErrorMessage = "Stock adjustment must be positive")]
        public int StockAdjustment { get; set; } = 0;

        public IFormFile? ImageFile { get; set; }
    }
}