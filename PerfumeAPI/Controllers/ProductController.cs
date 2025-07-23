using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PerfumeAPI.Data;
using PerfumeAPI.Models.Entities;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PerfumeAPI.Controllers
{
    public class ProductController : Controller
    {
        private readonly AppDbContext _context;

        public ProductController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string sortOrder = "", string fragranceFamily = "")
        {
            ViewBag.PriceSortParam = sortOrder == "price_asc" ? "price_desc" : "price_asc";
            ViewBag.NewestSortParam = "newest";
            ViewBag.FeaturedSortParam = "featured";

            IQueryable<Product> products = _context.Products
                .Include(p => p.Comments);

            if (!string.IsNullOrEmpty(fragranceFamily))
            {
                products = products.Where(p => p.FragranceFamily == fragranceFamily);
            }

            products = sortOrder switch
            {
                "price_asc" => products.OrderBy(p => p.Price),
                "price_desc" => products.OrderByDescending(p => p.Price),
                "newest" => products.OrderByDescending(p => p.CreatedAt),
                "featured" => products.Where(p => p.IsFeatured).OrderByDescending(p => p.CreatedAt),
                _ => products.OrderBy(p => p.Name)
            };

            ViewBag.FragranceFamilies = await _context.Products
                .Where(p => p.FragranceFamily != null)
                .Select(p => p.FragranceFamily)
                .Distinct()
                .ToListAsync();

            return View(await products.ToListAsync());
        }

        public async Task<IActionResult> Details(int id)
        {
            var product = await _context.Products
                .Include(p => p.Comments)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddComment(int productId, string text, int rating)
        {
            if (string.IsNullOrEmpty(text))
            {
                return BadRequest("Comment text is required");
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
                ProductId = productId,
                UserId = userId,
                Text = text,
                Rating = rating,
                CreatedAt = DateTime.UtcNow,
                User = user
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = productId });
        }
    }
}