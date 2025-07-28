using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PerfumeAPI.Models.Entities
{
    public class Comment
    {
        public int Id { get; set; }

        [Range(1, 5)]
        public int Rating { get; set; }

        [Required]
        public string Text { get; set; } = string.Empty;

        // Make ProductId nullable
        public int? ProductId { get; set; }

        // Navigation property optional
        public Product? Product { get; set; }

        public string? UserId { get; set; }
        public User? User { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public List<CommentImage>? Images { get; set; }
    }
}
