using Microsoft.EntityFrameworkCore;
using PerfumeAPI.Data;
using PerfumeAPI.Models.DTOs;
using PerfumeAPI.Models.Entities;
using PerfumeAPI.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;

namespace PerfumeAPI.Services
{
    public class PerfumeService : IPerfumeService
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public PerfumeService(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task<IEnumerable<Product>> GetAllProductsAsync()
        {
            return await _context.Products
                .Include(p => p.Comments)
                .ThenInclude(c => c.User)
                .ToListAsync();
        }

        public async Task<Product?> GetProductByIdAsync(int id)
        {
            return await _context.Products
                .Include(p => p.Comments)
                .ThenInclude(c => c.User)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Product> CreateProductAsync(ProductCreateDto productDto)
        {
            if (productDto == null)
            {
                throw new ArgumentNullException(nameof(productDto));
            }

            var product = new Product
            {
                Name = productDto.Name,
                Description = productDto.Description,
                Price = productDto.Price,
                ShippingCost = productDto.ShippingCost,
                FragranceType = productDto.FragranceType,
                Size = productDto.Size,
                CreatedAt = DateTime.UtcNow
            };

            if (productDto.ImageFile != null)
            {
                product.ImageUrl = await SaveImageAsync(productDto.ImageFile);
            }

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return product;
        }

        public async Task UpdateProductAsync(int id, ProductUpdateDto productDto)
        {
            if (productDto == null)
            {
                throw new ArgumentNullException(nameof(productDto));
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                throw new KeyNotFoundException($"Product with ID {id} not found");
            }

            product.Name = productDto.Name ?? product.Name;
            product.Description = productDto.Description ?? product.Description;
            product.Price = productDto.Price;
            product.ShippingCost = productDto.ShippingCost;
            product.FragranceType = productDto.FragranceType ?? product.FragranceType;
            product.Size = productDto.Size ?? product.Size;
            product.UpdatedAt = DateTime.UtcNow;

            if (productDto.ImageFile != null)
            {
                if (!string.IsNullOrEmpty(product.ImageUrl))
                {
                    DeleteImage(product.ImageUrl);
                }
                product.ImageUrl = await SaveImageAsync(productDto.ImageFile);
            }

            await _context.SaveChangesAsync();
        }

        public async Task DeleteProductAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                throw new KeyNotFoundException($"Product with ID {id} not found");
            }

            if (!string.IsNullOrEmpty(product.ImageUrl))
            {
                DeleteImage(product.ImageUrl);
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
        }

        public async Task AddCommentAsync(CommentCreateDto commentDto, string userId)
        {
            if (commentDto == null)
            {
                throw new ArgumentNullException(nameof(commentDto));
            }

            var comment = new Comment
            {
                ProductId = commentDto.ProductId,
                UserId = userId,
                Rating = commentDto.Rating,
                Text = commentDto.Text,
                CreatedAt = DateTime.UtcNow
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            if (commentDto.Images != null && commentDto.Images.Count > 0)
            {
                foreach (var imageFile in commentDto.Images)
                {
                    if (imageFile != null)
                    {
                        var imageUrl = await SaveImageAsync(imageFile);
                        _context.CommentImages.Add(new CommentImage
                        {
                            CommentId = comment.Id,
                            ImageUrl = imageUrl
                        });
                    }
                }
                await _context.SaveChangesAsync();
            }
        }

        private async Task<string> SaveImageAsync(IFormFile imageFile)
        {
            if (imageFile == null || imageFile.Length == 0)
            {
                throw new ArgumentException("Invalid image file");
            }

            var uploadsFolder = Path.Combine(_env.WebRootPath, "images", "products");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(fileStream);
            }

            return $"/images/products/{uniqueFileName}";
        }

        private void DeleteImage(string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl))
            {
                return;
            }

            var imagePath = Path.Combine(_env.WebRootPath, imageUrl.TrimStart('/'));
            if (File.Exists(imagePath))
            {
                File.Delete(imagePath);
            }
        }
    }
}
