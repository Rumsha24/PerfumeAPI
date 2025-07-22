using PerfumeAPI.Models.DTOs; 
using PerfumeAPI.Models.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PerfumeAPI.Services.Interfaces
{
    public interface IPerfumeService
    {
        Task<IEnumerable<Product>> GetAllProductsAsync();
        Task<Product?> GetProductByIdAsync(int id);
        Task<Product> CreateProductAsync(ProductCreateDto productDto);
        Task UpdateProductAsync(int id, ProductUpdateDto productDto); // This line references ProductUpdateDto
        Task DeleteProductAsync(int id);
        Task AddCommentAsync(CommentCreateDto commentDto, string userId);
    }
}
