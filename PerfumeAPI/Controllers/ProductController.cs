using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PerfumeAPI.Data;
using PerfumeAPI.Models.Entities;
using System.Security.Claims;
using PerfumeAPI.Services;

namespace PerfumeAPI.Controllers
{
    public class ProductController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IImageService _imageService;
        private readonly ILogger<ProductController> _logger;

        public ProductController(
            AppDbContext context,
            IImageService imageService,
            ILogger<ProductController> logger)
        {
            _context = context;
            _imageService = imageService;
            _logger = logger;
        }

        public async Task<IActionResult> Index(string sortOrder = "", string fragranceFamily = "")
        {
            _logger.LogInformation("Loading products with sort: {Sort}, family: {Family}",
                sortOrder, fragranceFamily);

            ViewBag.PriceSortParam = sortOrder == "price_asc" ? "price_desc" : "price_asc";
            ViewBag.NewestSortParam = "newest";
            ViewBag.FeaturedSortParam = "featured";

            var products = _context.Products
                .Include(p => p.Comments)
                .AsNoTracking();

            if (!string.IsNullOrEmpty(fragranceFamily))
            {
                products = products.Where(p =>
                    p.FragranceFamily != null &&
                    p.FragranceFamily == fragranceFamily);
            }

            products = sortOrder switch
            {
                "price_asc" => products.OrderBy(p => p.Price),
                "price_desc" => products.OrderByDescending(p => p.Price),
                "newest" => products.OrderByDescending(p => p.CreatedAt),
                "featured" => products.Where(p => p.IsFeatured)
                                    .OrderByDescending(p => p.CreatedAt),
                _ => products.OrderBy(p => p.Name)
            };

            return View(await products.ToListAsync());
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var product = await _context.Products
                .Include(p => p.Comments)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                _logger.LogWarning("Product {Id} not found", id);
                return NotFound();
            }

            return View(product);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddComment(int productId, string text, int rating)
        {
            if (string.IsNullOrEmpty(text) || rating < 1 || rating > 5)
            {
                TempData["ErrorMessage"] = "Please provide valid comment text and rating (1-5 stars)";
                return RedirectToAction("Details", new { id = productId });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var comment = new Comment
            {
                ProductId = productId,
                UserId = userId,
                Text = text,
                Rating = rating,
                CreatedAt = DateTime.UtcNow
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Thank you for your review!";
            return RedirectToAction("Details", new { id = productId });
        }
    }
}