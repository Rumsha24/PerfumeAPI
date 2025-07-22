using PerfumeAPI.Models.DTOs;
using PerfumeAPI.Models.Entities;
using System.Threading.Tasks;

namespace PerfumeAPI.Services.Interfaces
{
    public interface ICartService
    {
        Task<Cart> GetUserCartAsync(string userId);
        Task AddToCartAsync(string userId, int productId, int quantity = 1);
        Task RemoveFromCartAsync(string userId, int productId);
        Task UpdateQuantityAsync(string userId, int productId, int quantity);
        Task ClearCartAsync(string userId);
        Task<int> GetCartItemCountAsync(string userId);
        Task<CartDto> GetCartDtoAsync(string userId);
    }
}