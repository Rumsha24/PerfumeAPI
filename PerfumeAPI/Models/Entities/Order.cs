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

        public User? User { get; set; }

        [Required]
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        [Required]
        [StringLength(500)]
        public string ShippingAddress { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Processing";

        public DateTime? ShippedDate { get; set; }
        public DateTime? DeliveredDate { get; set; }
        public DateTime? CancelledDate { get; set; }

        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();

        public void GenerateOrderNumber()
        {
            OrderNumber = Guid.NewGuid().ToString("N")[..8].ToUpper();
        }
    }
}