using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PerfumeAPI.Data;
using PerfumeAPI.Models;
using PerfumeAPI.Models.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace PerfumeAPI.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ILogger<HomeController> _logger;

        public HomeController(AppDbContext context, ILogger<HomeController> logger)
        {
            _context = context;
            _logger = logger;
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
            var products = await _context.Products
                .Include(p => p.Comments)
                .ToListAsync();

            return Json(products.Select(p => new {
                p.Id,
                p.Name,
                p.IsFeatured,
                p.ImageUrl,
                p.Price,
                HasImage = !string.IsNullOrEmpty(p.ImageUrl),
                ReviewCount = p.Comments.Count,
                AverageRating = p.Comments.Any() ? p.Comments.Average(c => c.Rating) : 0
            }));
        }
    }
}