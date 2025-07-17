using System.ComponentModel.DataAnnotations;

namespace PerfumeAPI.Models.DTOs
{
    public class ProductDto
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        public string Description { get; set; }

        public string ImageUrl { get; set; }

        [Required]
        [Range(0.01, 10000)]
        public decimal Price { get; set; }

        [Required]
        [Range(0, 1000)]
        public decimal ShippingCost { get; set; }

        [Required]
        public string FragranceType { get; set; }

        [Required]
        public string Size { get; set; }

        public double AverageRating { get; set; }
        public int ReviewCount { get; set; }
    }

    public class ProductCreateDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        [Range(0.01, 10000)]
        public decimal Price { get; set; }

        [Required]
        [Range(0, 1000)]
        public decimal ShippingCost { get; set; }

        [Required]
        public string FragranceType { get; set; }

        [Required]
        public string Size { get; set; }

        public IFormFile? ImageFile { get; set; }
    }
}