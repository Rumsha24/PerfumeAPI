using PerfumeAPI.Services;
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

        // Inventory Management Properties
        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Stock quantity cannot be negative")]
        public int StockQuantity { get; set; } = 0;

        [Required]
        public bool IsInStock { get; set; } = true;

        // Low stock threshold (not mapped to database)
        [NotMapped]
        public bool IsLowStock => StockQuantity > 0 && StockQuantity <= LowStockThreshold;

        [NotMapped]
        public int LowStockThreshold { get; set; } = 5;

        // Audit Fields
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation Properties
        public virtual ICollection<Comment> Comments { get; set; } = new HashSet<Comment>();
        public virtual ICollection<CartItem> CartItems { get; set; } = new HashSet<CartItem>();
        public virtual ICollection<OrderItem> OrderItems { get; set; } = new HashSet<OrderItem>();

        // Computed Properties
        [NotMapped]
        public double AverageRating => Comments?.Average(c => c.Rating) ?? 0;

        [NotMapped]
        public int ReviewCount => Comments?.Count ?? 0;

        // Inventory Management Methods
        public void UpdateStock(int quantity)
        {
            StockQuantity += quantity;
            IsInStock = StockQuantity > 0;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Restock(int quantity)
        {
            if (quantity <= 0)
                throw new ArgumentException("Restock quantity must be positive");
            UpdateStock(quantity);
        }

        public void ReduceStock(int quantity)
        {
            if (quantity <= 0)
                throw new ArgumentException("Reduction quantity must be positive");
            if (quantity > StockQuantity)
                throw new InsufficientStockException(Id, StockQuantity, quantity);
            UpdateStock(-quantity);
        }
    }
}