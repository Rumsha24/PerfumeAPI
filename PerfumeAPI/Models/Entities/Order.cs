using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PerfumeAPI.Models.Entities
{
    public class Order
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        public string OrderNumber { get; set; } = Guid.NewGuid().ToString("N")[..8].ToUpper();

        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        [DataType(DataType.DateTime)]
        public DateTime? PaymentDate { get; set; }

        [Required]
        [StringLength(500)]
        public string ShippingAddress { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0, 100000)]
        public decimal TotalAmount { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Pending Payment"; // Changed from "Processing"

        [DataType(DataType.DateTime)]
        public DateTime? ShippedDate { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? DeliveredDate { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? CancelledDate { get; set; }

        public virtual ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();

        public void GenerateOrderNumber()
        {
            OrderNumber = Guid.NewGuid().ToString("N")[..8].ToUpper();
        }

        // Helper method to calculate total items
        [NotMapped]
        public int TotalItems => Items?.Sum(i => i.Quantity) ?? 0;
    }
}