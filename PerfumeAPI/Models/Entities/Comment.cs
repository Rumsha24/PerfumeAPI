using System.ComponentModel.DataAnnotations;

namespace PerfumeAPI.Models.Entities
{
    public class Comment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }

        [Required]
        [StringLength(2000)]
        public string Text { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Foreign keys
        public int ProductId { get; set; }
        public string UserId { get; set; }

        // Navigation properties
        public Product Product { get; set; }
        public User User { get; set; }
        public ICollection<CommentImage> Images { get; set; } = new List<CommentImage>();
    }
}