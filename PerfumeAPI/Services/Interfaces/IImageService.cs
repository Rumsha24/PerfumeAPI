using Microsoft.AspNetCore.Http;

namespace PerfumeAPI.Services
{
    public interface IImageService
    {
        Task<string> SaveImage(IFormFile imageFile);
        Task<bool> DeleteImage(string imageUrl);
    }
}