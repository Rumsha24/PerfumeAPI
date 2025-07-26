using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PerfumeAPI.Data;
using PerfumeAPI.Models.Entities;
using PerfumeAPI.Services;
using System.Security.Claims;

namespace PerfumeAPI.Controllers
{
    [Route("products")]
    public class ProductController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ProductController> _logger;
        private readonly IImageService _imageService;

        public ProductController(
            AppDbContext context,
            ILogger<ProductController> logger,
            IImageService imageService)
        {
            _context = context;
            _logger = logger;
            _imageService = imageService;
        }

        // GET: products/
        [HttpGet]
        [Route("")]
        [Route("index")]
        [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any)]
        public async Task<IActionResult> Index(
            string sortOrder = "name",
            string family = "",
            string search = "",
            int page = 1,
            int pageSize = 12)
        {
            try
            {
                ViewBag.SortOrder = sortOrder;
                ViewBag.CurrentFamily = family;
                ViewBag.CurrentSearch = search;
                ViewBag.CurrentPage = page;
                ViewBag.PageSize = pageSize;

                var query = _context.Products
                    .Include(p => p.Comments)
                    .AsNoTracking()
                    .AsQueryable();

                // Apply filters
                if (!string.IsNullOrEmpty(family))
                {
                    query = query.Where(p => p.FragranceFamily == family);
                }

                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(p =>
                        p.Name.Contains(search) ||
                        p.Description.Contains(search));
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

                // Pagination
                var totalCount = await query.CountAsync();
                var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
                page = Math.Max(1, Math.Min(page, totalPages));
                ViewBag.TotalPages = totalPages;

                var products = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                ViewBag.AvailableFamilies = await _context.Products
                    .Where(p => !string.IsNullOrEmpty(p.FragranceFamily))
                    .Select(p => p.FragranceFamily!)
                    .Distinct()
                    .ToListAsync();

                return View(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading products");
                return StatusCode(500, "An error occurred while loading products");
            }
        }

        // GET: products/details/5
        [HttpGet("{id:int}")]
        [Route("details/{id:int}")]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var product = await _context.Products
                    .Include(p => p.Comments)
                        .ThenInclude(c => c.User)
                    .Include(p => p.Comments)
                        .ThenInclude(c => c.Images)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (product == null)
                {
                    return NotFound();
                }

                ViewBag.AverageRating = product.Comments.Any() ?
                    product.Comments.Average(c => c.Rating) : 0;

                return View(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching product details for ID: {Id}", id);
                return StatusCode(500, "An error occurred while loading product details");
            }
        }

        // GET: products/create
        [Authorize(Roles = "Admin")]
        [HttpGet("create")]
        public IActionResult Create()
        {
            return View(new Product());
        }

        // POST: products/create
        [Authorize(Roles = "Admin")]
        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Name,Description,Price,ShippingCost,FragranceType,FragranceFamily,Size,StockQuantity,IsFeatured")] Product product,
            IFormFile imageFile)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Handle image upload
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        product.ImageUrl = await _imageService.SaveImage(imageFile);
                    }
                    else
                    {
                        product.ImageUrl = "/images/default-perfume.jpg";
                    }

                    product.CreatedAt = DateTime.UtcNow;
                    product.IsInStock = product.StockQuantity > 0;

                    _context.Add(product);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = $"{product.Name} created successfully!";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product");
                ModelState.AddModelError("", "An error occurred while creating the product");
            }

            return View(product);
        }

        // GET: products/edit/5
        [Authorize(Roles = "Admin")]
        [HttpGet("edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        // POST: products/edit/5
        [Authorize(Roles = "Admin")]
        [HttpPost("edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("Id,Name,Description,Price,ShippingCost,FragranceType,FragranceFamily,Size,StockQuantity,IsFeatured,ImageUrl")] Product product,
            IFormFile imageFile)
        {
            if (id != product.Id)
            {
                return NotFound();
            }

            try
            {
                if (ModelState.IsValid)
                {
                    var existingProduct = await _context.Products.FindAsync(id);
                    if (existingProduct == null)
                    {
                        return NotFound();
                    }

                    // Handle image update
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        // Delete old image if it exists and isn't the default
                        if (!string.IsNullOrEmpty(existingProduct.ImageUrl) &&
                            !existingProduct.ImageUrl.Contains("default-perfume"))
                        {
                            await _imageService.DeleteImage(existingProduct.ImageUrl);
                        }

                        existingProduct.ImageUrl = await _imageService.SaveImage(imageFile);
                    }

                    // Update other properties
                    existingProduct.Name = product.Name;
                    existingProduct.Description = product.Description;
                    existingProduct.Price = product.Price;
                    existingProduct.ShippingCost = product.ShippingCost;
                    existingProduct.FragranceType = product.FragranceType;
                    existingProduct.FragranceFamily = product.FragranceFamily;
                    existingProduct.Size = product.Size;
                    existingProduct.StockQuantity = product.StockQuantity;
                    existingProduct.IsFeatured = product.IsFeatured;
                    existingProduct.IsInStock = product.StockQuantity > 0;
                    existingProduct.UpdatedAt = DateTime.UtcNow;

                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"{product.Name} updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product ID: {ProductId}", id);
                ModelState.AddModelError("", "An error occurred while updating the product");
            }

            return View(product);
        }

        // GET: products/delete/5
        [Authorize(Roles = "Admin")]
        [HttpGet("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: products/delete/5
        [Authorize(Roles = "Admin")]
        [HttpPost("delete/{id}")]
        [ValidateAntiForgeryToken]
        [ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var product = await _context.Products.FindAsync(id);
                if (product == null)
                {
                    return NotFound();
                }

                // Delete associated image if it exists and isn't the default
                if (!string.IsNullOrEmpty(product.ImageUrl) &&
                    !product.ImageUrl.Contains("default-perfume"))
                {
                    await _imageService.DeleteImage(product.ImageUrl);
                }

                _context.Products.Remove(product);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"{product.Name} deleted successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product ID: {ProductId}", id);
                TempData["ErrorMessage"] = "An error occurred while deleting the product";
                return RedirectToAction(nameof(Delete), new { id });
            }
        }

        // POST: products/comment
        [Authorize]
        [HttpPost("comment")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment([FromForm] int productId, [FromForm] string text, [FromForm] int rating)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(text) || rating < 1 || rating > 5)
                {
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

                TempData["SuccessMessage"] = "Thank you for your review!";
                return RedirectToAction("Details", new { id = productId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding comment for product {ProductId}", productId);
                TempData["ErrorMessage"] = "Failed to add your review. Please try again.";
                return RedirectToAction("Details", new { id = productId });
            }
        }

        // GET: products/debug
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
}