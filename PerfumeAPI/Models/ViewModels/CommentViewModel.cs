// Models/ViewModels/CommentViewModel.cs
using System.Collections.Generic;
using PerfumeAPI.Models.Entities;
using PerfumeAPI.Models.DTOs;

namespace PerfumeAPI.Models.ViewModels
{
    public class CommentViewModel
    {
        public IEnumerable<Comment> Comments { get; set; } = new List<Comment>();
        public CommentCreateDTO NewComment { get; set; } = new CommentCreateDTO();
        public int ProductId { get; set; }
    }
}
