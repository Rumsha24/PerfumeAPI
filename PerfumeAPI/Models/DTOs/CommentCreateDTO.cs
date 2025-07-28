using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace PerfumeAPI.Models.DTOs
{
    public class CommentCreateDTO
    {
        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }

        [Required]
        [StringLength(2000)]
        public string Text { get; set; } = string.Empty;

        // Make ProductId nullable (optional)
        public int ProductId { get; set; }

        public List<IFormFile> Images { get; set; } = new List<IFormFile>();
    }
}
