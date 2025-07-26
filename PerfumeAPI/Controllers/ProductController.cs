using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PerfumeAPI.Data;
using PerfumeAPI.Models.Entities;
using System.Diagnostics;
using System.Security.Claims;

namespace PerfumeAPI.Controllers
{
    [Route("products")]
    public class ProductController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ProductController> _logger;

        public ProductController(
            AppDbContext context,
            ILogger<ProductController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        [Route("")]
        [Route("index")]
        [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any)]
        public async Task<IActionResult> Index(
            string sortOrder = "name",
            string fragranceFamily = "",
            string searchQuery = "",
            int page = 1,
            int pageSize = 12)
        {
            try
            {
                _logger.LogInformation("Loading products with sort: {Sort}, family: {Family}, search: {Search}, page: {Page}",
                    sortOrder, fragranceFamily, searchQuery, page);

                // Base query with includes
                var query = _context.Products
                    .Include(p => p.Comments)
                    .AsNoTracking()
                    .AsQueryable();

                // Apply filters
                if (!string.IsNullOrEmpty(fragranceFamily))
                {
                    query = query.Where(p => p.FragranceFamily == fragranceFamily);
                }

                if (!string.IsNullOrEmpty(searchQuery))
                {
                    query = query.Where(p =>
                        p.Name.Contains(searchQuery) ||
                        p.Description.Contains(searchQuery));
                }

                // Apply sorting
                query = sortOrder.ToLower() switch
                {
                    "price_asc" => query.OrderBy(p => p.Price),
                    "price_desc" => query.OrderByDescending(p => p.Price),
                    "newest" => query.OrderByDescending(p => p.CreatedAt),
                    "featured" => query.Where(p => p.IsFeatured)
                                     .OrderByDescending(p => p.CreatedAt),
                    _ => query.OrderBy(p => p.Name) // Default
                };

                // Get total count for pagination
                var totalCount = await query.CountAsync();
                var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

                // Validate page number
                page = Math.Max(1, Math.Min(page, totalPages));

                // Execute query
                var products = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                _logger.LogInformation("Retrieved {Count} products out of {Total}", products.Count, totalCount);

                // Prepare view model
                var viewModel = new ProductIndexViewModel
                {
                    Products = products,
                    CurrentPage = page,
                    TotalPages = totalPages,
                    SortOrder = sortOrder,
                    FragranceFamily = fragranceFamily,
                    SearchQuery = searchQuery,
                    AvailableFamilies = await _context.Products
                        .Where(p => !string.IsNullOrEmpty(p.FragranceFamily))
                        .Select(p => p.FragranceFamily!)
                        .Distinct()
                        .ToListAsync()
                };

                // Debug information
                ViewData["DebugInfo"] = new
                {
                    TotalProducts = totalCount,
                    Database = _context.Database.GetDbConnection().Database
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading products");
                return StatusCode(500, "An error occurred while loading products");
            }
        }

        [HttpGet("{id:int}")]
        [Route("details/{id:int}")]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                _logger.LogInformation("Fetching product details for ID: {Id}", id);

                var product = await _context.Products
                    .Include(p => p.Comments)
                        .ThenInclude(c => c.User)
                    .Include(p => p.Comments)
                        .ThenInclude(c => c.Images)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (product == null)
                {
                    _logger.LogWarning("Product not found: {Id}", id);
                    return NotFound();
                }

                // Calculate average rating
                if (product.Comments.Any())
                {
                    ViewBag.AverageRating = product.Comments
                        .Average(c => c.Rating);
                }

                return View(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching product details for ID: {Id}", id);
                return StatusCode(500, "An error occurred while loading product details");
            }
        }

        [HttpPost("comment")]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(
            [FromForm] int productId,
            [FromForm] string text,
            [FromForm] int rating)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(text) || rating < 1 || rating > 5)
                {
                    _logger.LogWarning("Invalid comment submission for product {Id}", productId);
                    TempData["ErrorMessage"] = "Please provide valid comment text and rating (1-5 stars)";
                    return RedirectToAction("Details", new { id = productId });
                }

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null)
                {
                    return Unauthorized();
                }

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

                _logger.LogInformation("New comment added for product {Id} by user {UserId}",
                    productId, userId);

                TempData["SuccessMessage"] = "Thank you for your review!";
                return RedirectToAction("Details", new { id = productId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding comment for product {Id}", productId);
                TempData["ErrorMessage"] = "Failed to add your review. Please try again.";
                return RedirectToAction("Details", new { id = productId });
            }
        }

        [HttpGet("debug")]
        [ResponseCache(NoStore = true)]
        public async Task<IActionResult> DebugInfo()
        {
            try
            {
                var dbInfo = new
                {
                    Database = _context.Database.GetDbConnection().Database,
                    Server = _context.Database.GetDbConnection().DataSource,
                    ProductCount = await _context.Products.CountAsync(),
                    CanConnect = await _context.Database.CanConnectAsync()
                };

                return Json(dbInfo);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }
    }

    public class ProductIndexViewModel
    {
        public List<Product> Products { get; set; } = new List<Product>();
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
        public string SortOrder { get; set; } = "name";
        public string FragranceFamily { get; set; } = string.Empty;
        public string SearchQuery { get; set; } = string.Empty;
        public List<string> AvailableFamilies { get; set; } = new List<string>();
    }
}