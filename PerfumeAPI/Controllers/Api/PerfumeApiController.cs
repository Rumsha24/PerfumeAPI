using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PerfumeAPI.Data;
using PerfumeAPI.Models.DTOs;
using PerfumeAPI.Models.Entities;

namespace PerfumeAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PerfumeApiController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public PerfumeApiController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetProducts()
        {
            var products = await _context.Products
                .Include(p => p.Comments)
                .Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    ImageUrl = p.ImageUrl,
                    Price = p.Price,
                    ShippingCost = p.ShippingCost,
                    FragranceType = p.FragranceType,
                    Size = p.Size,
                    AverageRating = p.Comments.Any() ? p.Comments.Average(c => c.Rating) : 0,
                    ReviewCount = p.Comments.Count
                })
                .ToListAsync();

            return Ok(products);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProductDto>> GetProduct(int id)
        {
            var product = await _context.Products
                .Include(p => p.Comments)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
                return NotFound();

            var dto = new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                ImageUrl = product.ImageUrl,
                Price = product.Price,
                ShippingCost = product.ShippingCost,
                FragranceType = product.FragranceType,
                Size = product.Size,
                AverageRating = product.Comments.Any() ? product.Comments.Average(c => c.Rating) : 0,
                ReviewCount = product.Comments.Count
            };

            return Ok(dto);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<Product>> CreateProduct([FromForm] ProductCreateDto dto)
        {
            if (dto.ImageFile != null)
            {
                var ext = Path.GetExtension(dto.ImageFile.FileName).ToLower();
                if (ext != ".jpg" && ext != ".png")
                {
                    return BadRequest("Only JPG or PNG files are allowed.");
                }

                var fileName = Guid.NewGuid() + ext;
                var filePath = Path.Combine(_env.WebRootPath, "images", fileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await dto.ImageFile.CopyToAsync(stream);

                var product = new Product
                {
                    Name = dto.Name,
                    Description = dto.Description,
                    Price = dto.Price,
                    ShippingCost = dto.ShippingCost,
                    FragranceType = dto.FragranceType,
                    Size = dto.Size,
                    ImageUrl = "/images/" + fileName
                };

                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
            }

            return BadRequest("Image is required.");
        }
    }
}
