using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace PerfumeAPI.Models.Entities
{
    public class User : IdentityUser
    {
        [Required]
        [StringLength(100)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(100)]
        public string LastName { get; set; }

        [Required]
        [StringLength(500)]
        public string ShippingAddress { get; set; }

        public DateTime MemberSince { get; set; } = DateTime.UtcNow;
        public DateTime? LastLogin { get; set; }

        // Navigation properties
        public Cart Cart { get; set; }  // This should already be present
        public ICollection<Order> Orders { get; set; } = new List<Order>();
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    }
}
