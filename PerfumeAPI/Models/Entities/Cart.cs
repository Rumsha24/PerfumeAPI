using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PerfumeAPI.Models.Entities
{
    public class Cart
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(450)] // Matches IdentityUser.Id length
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        public virtual ICollection<CartItem> Items { get; set; } = new HashSet<CartItem>();

        [NotMapped]
        public decimal Subtotal => Items?.Sum(i => i.Quantity * i.Product.Price) ?? 0;

        [NotMapped]
        public decimal ShippingTotal => Items?.FirstOrDefault()?.Product?.ShippingCost ?? 0;

        [NotMapped]
        public decimal GrandTotal => Subtotal + ShippingTotal;
    }
}