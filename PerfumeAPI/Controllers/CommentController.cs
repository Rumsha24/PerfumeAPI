using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PerfumeAPI.Data;
using PerfumeAPI.Models.DTOs;
using PerfumeAPI.Models.Entities;
using System.Security.Claims;

namespace PerfumeAPI.Controllers
{
    [Authorize]
    public class CommentController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public CommentController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        [HttpPost]
        public async Task<IActionResult> Create(CommentCreateDto dto)
        {
            if (!ModelState.IsValid || dto == null)
            {
                return RedirectToAction("Details", "Product", new { id = dto?.ProductId });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return Unauthorized();
            }

            var comment = new Comment
            {
                ProductId = dto.ProductId,
                UserId = userId,
                Text = dto.Text,
                Rating = dto.Rating,
                CreatedAt = DateTime.UtcNow
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            if (dto.Images != null && dto.Images.Any())
            {
                foreach (var image in dto.Images.Take(3))
                {
                    if (image.Length > 0)
                    {
                        var uploadsFolder = Path.Combine(_env.WebRootPath, "images", "comments");
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }

                        var uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(image.FileName)}";
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await image.CopyToAsync(fileStream);
                        }

                        var commentImage = new CommentImage
                        {
                            CommentId = comment.Id,
                            ImageUrl = $"/images/comments/{uniqueFileName}"
                        };

                        _context.CommentImages.Add(commentImage);
                    }
                }
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Details", "Product", new { id = dto.ProductId });
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var comment = await _context.Comments
                .Include(c => c.Images)
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

            if (comment == null)
            {
                return NotFound();
            }

            foreach (var image in comment.Images)
            {
                var imagePath = Path.Combine(_env.WebRootPath, image.ImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Product", new { id = comment.ProductId });
        }
    }
}