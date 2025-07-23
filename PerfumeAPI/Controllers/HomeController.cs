using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PerfumeAPI.Data;
using PerfumeAPI.Models;
using PerfumeAPI.Models.Entities;
using System.Diagnostics;

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
                var featuredProducts = await _context.Products
                    .Where(p => p.IsFeatured)
                    .OrderByDescending(p => p.CreatedAt)
                    .Take(4)
                    .ToListAsync();

                ViewBag.FragranceFamilies = await _context.Products
                    .Where(p => p.FragranceFamily != null)
                    .Select(p => p.FragranceFamily)
                    .Distinct()
                    .ToListAsync();

                return View(featuredProducts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading featured products");
                return View(new List<Product>());
            }
        }

        public IActionResult Story()
        {
            ViewBag.Awards = new List<string>
            {
                "FiFi Awards 2023 - Best New Fragrance",
                "Luxe Beauty Prize 2022",
                "Artisan Perfumer of the Year 2011"
            };
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}