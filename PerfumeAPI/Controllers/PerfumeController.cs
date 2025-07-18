using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PerfumeAPI.Data;
using PerfumeAPI.Models.Entities;
using System.Security.Claims;

namespace PerfumeAPI.Controllers
{
    public class PerfumeController : Controller
    {
        private readonly AppDbContext _context;

        public PerfumeController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var perfumes = await _context.Products
                .Include(p => p.Comments)
                .ToListAsync();

            return View(perfumes);
        }

        public async Task<IActionResult> Details(int id)
        {
            var perfume = await _context.Products
                .Include(p => p.Comments)
                .ThenInclude(c => c.User)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (perfume == null)
                return NotFound();

            ViewBag.AverageRating = perfume.Comments.Any()
                ? perfume.Comments.Average(c => c.Rating)
                : 0;

            return View(perfume);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(int productId, string text, int rating)
        {
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Login", "Account");

            var comment = new Comment
            {
                ProductId = productId,
                UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                Text = text,
                Rating = rating,
                CreatedAt = DateTime.UtcNow
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = productId });
        }
    }
}
