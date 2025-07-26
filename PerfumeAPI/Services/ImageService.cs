using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace PerfumeAPI.Services
{
    public class ImageService : IImageService
    {
        private readonly IWebHostEnvironment _environment;

        public ImageService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public async Task<string> SaveImage(IFormFile imageFile)
        {
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "images", "uploads");
            var uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            Directory.CreateDirectory(uploadsFolder);
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(fileStream);
            }

            return "/images/uploads/" + uniqueFileName;
        }

        public async Task<bool> DeleteImage(string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl)) return false;

            var filePath = Path.Combine(_environment.WebRootPath, imageUrl.TrimStart('/'));
            if (File.Exists(filePath))
            {
                await Task.Run(() => File.Delete(filePath));
                return true;
            }
            return false;
        }
    }
}