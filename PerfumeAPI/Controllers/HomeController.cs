using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PerfumeAPI.Data;
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
                    .OrderByDescending(p => p.CreatedAt)
                    .Take(3)
                    .ToListAsync();

                return View(featuredProducts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading featured products");
                return View(new List<Product>()); // Return empty list if error occurs
            }
        }

        public IActionResult Story()
        {
            return View();
        }

        public async Task<IActionResult> Comments()
        {
            try
            {
                var comments = await _context.Comments
                    .Include(c => c.User)
                    .Include(c => c.Product)
                    .OrderByDescending(c => c.CreatedAt)
                    .ToListAsync();

                return View(comments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading comments");
                return View(new List<Comment>()); // Return empty list if error occurs
            }
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

    public class ErrorViewModel
    {
        public string? RequestId { get; set; }
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}