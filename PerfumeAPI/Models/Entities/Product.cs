using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PerfumeAPI.Models.Entities
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "text")]
        public string Description { get; set; } = string.Empty;

        [StringLength(500)]
        public string? ImageUrl { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0.01, 10000, ErrorMessage = "Price must be between 0.01 and 10,000")]
        public decimal Price { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0, 1000, ErrorMessage = "Shipping cost must be between 0 and 1,000")]
        public decimal ShippingCost { get; set; }

        [Required]
        [StringLength(50, ErrorMessage = "Fragrance type cannot exceed 50 characters")]
        public string FragranceType { get; set; } = string.Empty;

        [StringLength(50)]
        public string? FragranceFamily { get; set; }

        public bool IsFeatured { get; set; } = false;

        [Required]
        [StringLength(20, ErrorMessage = "Size cannot exceed 20 characters")]
        public string Size { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public virtual ICollection<Comment> Comments { get; set; } = new HashSet<Comment>();
        public virtual ICollection<CartItem> CartItems { get; set; } = new HashSet<CartItem>();
        public virtual ICollection<OrderItem> OrderItems { get; set; } = new HashSet<OrderItem>();

        [NotMapped]
        public double AverageRating => Comments?.Average(c => c.Rating) ?? 0;

        [NotMapped]
        public int ReviewCount => Comments?.Count ?? 0;
    }
}