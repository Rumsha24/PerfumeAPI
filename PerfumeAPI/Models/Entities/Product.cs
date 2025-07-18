using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PerfumeAPI.Models.Entities
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public required string Name { get; set; }

        [Required]
        [Column(TypeName = "text")]
        public required string Description { get; set; }

        // Made nullable by removing 'required' and adding '?'
        public string? ImageUrl { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0.01, 10000)]
        public decimal Price { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0, 1000)]
        public decimal ShippingCost { get; set; }

        [Required]
        public required string FragranceType { get; set; }

        [Required]
        public required string Size { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
