using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PerfumeAPI.Models.Entities
{
    public class Order
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string OrderNumber { get; set; } = Guid.NewGuid().ToString()[..8].ToUpper();

        [Required]
        [StringLength(500)]
        public string ShippingAddress { get; set; } = string.Empty;  // Initialize with empty string

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [Required]
        public string Status { get; set; } = "Processing"; // Processing, Shipped, Delivered, Cancelled

        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public DateTime? ShippedDate { get; set; }
        public DateTime? DeliveredDate { get; set; }
        public DateTime? CancelledDate { get; set; }

        // Foreign key
        [Required]
        public string UserId { get; set; } = string.Empty;  // Initialize with empty string

        // Navigation properties
        public virtual User User { get; set; } = null!;  // Mark as non-null with null-forgiving operator
        public virtual ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();

        // Constructor to ensure required properties are set
        public Order()
        {
            // Ensures all required string properties are initialized
            OrderNumber ??= Guid.NewGuid().ToString()[..8].ToUpper();
            ShippingAddress ??= string.Empty;
            Status ??= "Processing";
            UserId ??= string.Empty;
        }
    }
}