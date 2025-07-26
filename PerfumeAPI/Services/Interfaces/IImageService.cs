using Microsoft.AspNetCore.Http;

namespace PerfumeAPI.Services
{
    public interface IImageService
    {
        Task<string> SaveImage(IFormFile imageFile, string subFolder = "products");
        Task<bool> DeleteImage(string imageUrl);
        string GetImageUrl(string imagePath, string defaultImage = "default-perfume.jpg");
    }
}