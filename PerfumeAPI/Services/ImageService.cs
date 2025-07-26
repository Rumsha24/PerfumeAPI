using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace PerfumeAPI.Services
{
    public class ImageService : IImageService
    {
        private readonly IWebHostEnvironment _environment;
        private const string DefaultImage = "default-perfume.jpg";

        public ImageService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public async Task<string> SaveImage(IFormFile imageFile, string subFolder = "products")
        {
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "images", subFolder);
            Directory.CreateDirectory(uploadsFolder);

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(imageFile.FileName)}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            await using var fileStream = new FileStream(filePath, FileMode.Create);
            await imageFile.CopyToAsync(fileStream);

            return $"/images/{subFolder}/{fileName}";
        }

        public async Task<bool> DeleteImage(string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl)) return false;

            var filePath = Path.Combine(_environment.WebRootPath, imageUrl.TrimStart('/'));
            if (!File.Exists(filePath)) return false;

            await Task.Run(() => File.Delete(filePath));
            return true;
        }

        public string GetImageUrl(string imagePath, string defaultImage = DefaultImage)
        {
            if (string.IsNullOrEmpty(imagePath))
                return $"/images/{defaultImage}";

            var fullPath = Path.Combine(_environment.WebRootPath, imagePath.TrimStart('/'));
            return File.Exists(fullPath) ? $"/{imagePath.TrimStart('/')}" : $"/images/{defaultImage}";
        }
    }
}