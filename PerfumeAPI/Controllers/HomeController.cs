using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PerfumeAPI.Data;
using PerfumeAPI.Models;
using PerfumeAPI.Models.Entities;
using PerfumeAPI.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PerfumeAPI.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ILogger<HomeController> _logger;
        private readonly IImageService _imageService;

        public HomeController(
            AppDbContext context,
            ILogger<HomeController> logger,
            IImageService imageService)
        {
            _context = context;
            _logger = logger;
            _imageService = imageService;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                // Debug logging to verify data exists
                _logger.LogInformation($"Total products in database: {await _context.Products.CountAsync()}");

                // Get featured products with fallback
                var featuredProducts = await GetFeaturedProductsWithFallback();

                // Get top rated products with fallback
                var topRatedProducts = await GetTopRatedProductsWithFallback();

                // Ensure all products have valid image URLs
                foreach (var product in featuredProducts.Concat(topRatedProducts))
                {
                    product.ImageUrl = _imageService.GetImageUrl(
                        product.ImageUrl ?? string.Empty,
                        "default-perfume.jpg");
                }

                // Log results for debugging
                _logger.LogInformation($"Displaying {featuredProducts.Count} featured products");
                _logger.LogInformation($"Displaying {topRatedProducts.Count} top rated products");

                return View(new HomeViewModel
                {
                    FeaturedPerfumes = featuredProducts,
                    CustomerFavorites = topRatedProducts
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading homepage data");

                // Return empty lists to prevent null reference exceptions
                return View(new HomeViewModel
                {
                    FeaturedPerfumes = new List<Product>(),
                    CustomerFavorites = new List<Product>()
                });
            }
        }

        private async Task<List<Product>> GetFeaturedProductsWithFallback()
        {
            var products = await _context.Products
                .Where(p => p.IsFeatured)
                .OrderByDescending(p => p.CreatedAt)
                .Take(4)
                .ToListAsync();

            if (!products.Any())
            {
                _logger.LogInformation("No featured products found, using newest products as fallback");
                products = await _context.Products
                    .OrderByDescending(p => p.CreatedAt)
                    .Take(4)
                    .ToListAsync();
            }

            return products;
        }

        private async Task<List<Product>> GetTopRatedProductsWithFallback()
        {
            var products = await _context.Products
                .Include(p => p.Comments)
                .Where(p => p.Comments.Any())
                .OrderByDescending(p => p.Comments.Average(c => c.Rating))
                .ThenByDescending(p => p.Comments.Count)
                .Take(4)
                .ToListAsync();

            if (!products.Any())
            {
                _logger.LogInformation("No rated products found, using random products as fallback");
                products = await _context.Products
                    .OrderBy(p => Guid.NewGuid()) // Random order
                    .Take(4)
                    .ToListAsync();
            }

            return products;
        }

        // Debug endpoint to check raw product data
        [Route("/debug/products")]
        public async Task<IActionResult> DebugProducts()
        {
            try
            {
                var products = await _context.Products
                    .Include(p => p.Comments)
                    .ToListAsync();

                var result = products.Select(p => new {
                    p.Id,
                    p.Name,
                    p.IsFeatured,
                    OriginalImageUrl = p.ImageUrl,
                    ResolvedImageUrl = _imageService.GetImageUrl(p.ImageUrl ?? string.Empty, "default-perfume.jpg"),
                    HasImage = !string.IsNullOrEmpty(p.ImageUrl),
                    ReviewCount = p.Comments.Count,
                    AverageRating = p.Comments.Any() ? p.Comments.Average(c => c.Rating) : 0
                });

                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DebugProducts endpoint");
                return StatusCode(500, new { Error = "Failed to retrieve product data" });
            }
        }

        // New endpoint to get image URLs safely
        [Route("/api/products/{id}/image")]
        public async Task<IActionResult> GetProductImage(int id)
        {
            try
            {
                var product = await _context.Products
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (product == null)
                {
                    return NotFound(new { Message = "Product not found" });
                }

                var imageUrl = _imageService.GetImageUrl(
                    product.ImageUrl ?? string.Empty, // FIX: Ensure non-null argument
                    "default-perfume.jpg");

                return Ok(new { ImageUrl = imageUrl });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product image for ID: {ProductId}", id);
                return StatusCode(500, new { Error = "Failed to retrieve image" });
            }
        }

        public IActionResult Story()
        {
            return View();
        }
    }
}