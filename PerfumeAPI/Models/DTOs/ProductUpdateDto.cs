using System.ComponentModel.DataAnnotations;

namespace PerfumeAPI.Models.DTOs
{
    public class ProductUpdateDto
    {
        [Required]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string Description { get; set; } = string.Empty;

        [Required]
        [StringLength(50, ErrorMessage = "Fragrance type cannot exceed 50 characters")]
        public string FragranceType { get; set; } = string.Empty;

        [Required]
        [StringLength(20, ErrorMessage = "Size cannot exceed 20 characters")]
        public string Size { get; set; } = string.Empty;

        [Required]
        [Range(0.01, 10000, ErrorMessage = "Price must be between 0.01 and 10,000")]
        public decimal Price { get; set; }

        [Required]
        [Range(0, 1000, ErrorMessage = "Shipping cost must be between 0 and 1,000")]
        public decimal ShippingCost { get; set; }

        public IFormFile? ImageFile { get; set; }
        public bool? IsFeatured { get; set; }
    }
}