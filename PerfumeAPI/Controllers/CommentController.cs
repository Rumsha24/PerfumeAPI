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
            if (productId == null)
            {
                // Option A: Show a list of products with links to comment pages
                var products = await _context.Products.AsNoTracking().ToListAsync();
                return View("SelectProduct", products);
            }

            // Existing code to get comments for the product
            var comments = await _context.Comments
                .Where(c => c.ProductId == productId.Value)
                .Include(c => c.User)
                .Include(c => c.Images)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            ViewBag.ProductId = productId.Value;
            return View(comments);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] CommentCreateDTO commentDto)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.ProductId = commentDto.ProductId;
                var comments = await _context.Comments
                    .Where(c => c.ProductId == commentDto.ProductId)
                    .Include(c => c.Images)
                    .Include(c => c.User)
                    .OrderByDescending(c => c.CreatedAt)
                    .ToListAsync();

                return View("Index", comments);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Confirm product exists before adding comment
            var productExists = await _context.Products.AnyAsync(p => p.Id == commentDto.ProductId);
            if (!productExists)
            {
                ModelState.AddModelError("", "The product you are trying to review does not exist.");
                ViewBag.ProductId = commentDto.ProductId;
                var comments = await _context.Comments
                    .Where(c => c.ProductId == commentDto.ProductId)
                    .Include(c => c.Images)
                    .Include(c => c.User)
                    .OrderByDescending(c => c.CreatedAt)
                    .ToListAsync();

                return View("Index", comments);
            }

            var comment = new Comment
            {
                Text = commentDto.Text,
                Rating = commentDto.Rating,
                ProductId = commentDto.ProductId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                Images = new List<CommentImage>()
            };

            if (commentDto.Images != null && commentDto.Images.Count > 0)
            {
                var uploadsPath = Path.Combine(_env.WebRootPath, "uploads", "comments");
                Directory.CreateDirectory(uploadsPath);

                foreach (var image in commentDto.Images.Take(3))
                {
                    if (image != null && image.Length > 0 && image.Length <= 5 * 1024 * 1024) // max 5 MB
                    {
                        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(image.FileName)}";
                        var filePath = Path.Combine(uploadsPath, fileName);

                        using var stream = new FileStream(filePath, FileMode.Create);
                        await image.CopyToAsync(stream);

                        comment.Images.Add(new CommentImage
                        {
                            ImageUrl = $"/uploads/comments/{fileName}"
                        });
                    }
                }
            }

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            // Redirect back to comments for this product
            return RedirectToAction("Index", new { productId = commentDto.ProductId });
        }
    }
}
