using System.ComponentModel.DataAnnotations;

namespace PerfumeAPI.Models.DTOs
{
    public class CommentDto
    {
        public int Id { get; set; }

        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        public int Rating { get; set; }

        [StringLength(2000, ErrorMessage = "Comment cannot exceed 2000 characters")]
        public string Text { get; set; } = string.Empty;

        public string UserName { get; set; } = string.Empty;
        public string UserAvatar { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public List<string> ImageUrls { get; set; } = new List<string>();
    }

    public class CommentCreateDto
    {
        [Required]
        public int ProductId { get; set; }

        [Required]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        public int Rating { get; set; }

        [Required]
        [StringLength(2000, ErrorMessage = "Comment cannot exceed 2000 characters")]
        public string Text { get; set; } = string.Empty;

        [MaxLength(3, ErrorMessage = "Maximum 3 images allowed")]
        public List<IFormFile>? Images { get; set; }
    }
}