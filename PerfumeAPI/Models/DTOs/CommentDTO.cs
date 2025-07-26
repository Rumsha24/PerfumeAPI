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

    public class CommentCreateDTO
    {
        [Required(ErrorMessage = "Rating is required")]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        public int Rating { get; set; }

        [Required(ErrorMessage = "Review text is required")]
        [StringLength(2000, ErrorMessage = "Comment cannot exceed 2000 characters")]
        public string Text { get; set; } = string.Empty;

        [Required(ErrorMessage = "Product ID is required")]
        public int ProductId { get; set; }

        public List<IFormFile> Images { get; set; } = new List<IFormFile>();
    }
}