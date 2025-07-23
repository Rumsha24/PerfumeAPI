using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PerfumeAPI.Data;
using PerfumeAPI.Models.DTOs;
using PerfumeAPI.Models.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PerfumeAPI.Controllers.Api
{
    [Route("api/perfumes")]
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
            {
                return NotFound();
            }

            return Ok(new ProductDto
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
            });
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<Product>> CreateProduct([FromForm] ProductCreateDto dto)
        {
            if (dto.ImageFile == null)
            {
                return BadRequest("Image is required.");
            }

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
                ImageUrl = $"/images/{fileName}"
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] ProductUpdateDto dto)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            product.Name = dto.Name;
            product.Description = dto.Description;
            product.Price = dto.Price;
            product.ShippingCost = dto.ShippingCost;
            product.FragranceType = dto.FragranceType;
            product.Size = dto.Size;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}