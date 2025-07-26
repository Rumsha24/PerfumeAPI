using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
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
        private readonly UserManager<User> _userManager;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<CommentController> _logger;

        public CommentController(
            AppDbContext context,
            UserManager<User> userManager,
            IWebHostEnvironment env,
            ILogger<CommentController> logger)
        {
            _context = context;
            _userManager = userManager;
            _env = env;
            _logger = logger;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index(int? productId)
        {
            try
            {
                var query = _context.Comments
                    .Include(c => c.User)
                    .Include(c => c.Images)
                    .Include(c => c.Product)
                    .OrderByDescending(c => c.CreatedAt);

                if (productId.HasValue)
                {
                    query = query.Where(c => c.ProductId == productId.Value)
                                .OrderByDescending(c => c.CreatedAt);
                }

                var comments = await query.AsNoTracking().ToListAsync();
                return View(comments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading comments");
                return StatusCode(500, "An error occurred while loading comments");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] CommentCreateDTO commentDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                var comment = new Comment
                {
                    Text = commentDto.Text,
                    Rating = commentDto.Rating,
                    ProductId = commentDto.ProductId,
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow
                };

                // Handle image uploads
                if (commentDto.Images != null && commentDto.Images.Count > 0)
                {
                    var uploadsPath = Path.Combine(_env.WebRootPath, "uploads", "comments");
                    Directory.CreateDirectory(uploadsPath);

                    foreach (var image in commentDto.Images.Take(3))
                    {
                        if (image.Length > 0 && image.Length <= 5 * 1024 * 1024) // 5MB max
                        {
                            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(image.FileName)}";
                            var filePath = Path.Combine(uploadsPath, fileName);

                            using (var stream = new FileStream(filePath, FileMode.Create))
                            {
                                await image.CopyToAsync(stream);
                            }

                            comment.Images.Add(new CommentImage
                            {
                                ImageUrl = $"/uploads/comments/{fileName}"
                            });
                        }
                    }
                }

                _context.Comments.Add(comment);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index), new { productId = commentDto.ProductId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating comment");
                return StatusCode(500, "An error occurred while creating your comment");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                var comment = await _context.Comments
                    .Include(c => c.Images)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (comment == null || comment.UserId != userId)
                {
                    return NotFound();
                }

                // Delete associated images
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

                return RedirectToAction(nameof(Index), new { productId = comment.ProductId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting comment");
                return StatusCode(500, "An error occurred while deleting your comment");
            }
        }
    }
}