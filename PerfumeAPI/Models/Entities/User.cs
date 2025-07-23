using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PerfumeAPI.Models.Entities
{
    public class User : IdentityUser
    {
        [Required]
        [StringLength(100, ErrorMessage = "First name cannot exceed 100 characters")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100, ErrorMessage = "Last name cannot exceed 100 characters")]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [StringLength(500, ErrorMessage = "Shipping address cannot exceed 500 characters")]
        public string ShippingAddress { get; set; } = string.Empty;

        [StringLength(500)]
        public string? ProfileImageUrl { get; set; }

        public DateTime MemberSince { get; set; } = DateTime.UtcNow;
        public DateTime? LastLogin { get; set; }

        public virtual Cart Cart { get; set; } = null!;
        public virtual ICollection<Order> Orders { get; set; } = new HashSet<Order>();
        public virtual ICollection<Comment> Comments { get; set; } = new HashSet<Comment>();

        [NotMapped]
        public string FullName => $"{FirstName} {LastName}";
    }
}