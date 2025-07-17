namespace PerfumeAPI.Models.Entities
{
    public class CommentImage
    {
        public int Id { get; set; }
        public string ImageUrl { get; set; }

        // Foreign key
        public int CommentId { get; set; }

        // Navigation property
        public Comment Comment { get; set; }
    }
}