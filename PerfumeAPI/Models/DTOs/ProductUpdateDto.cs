namespace PerfumeAPI.Models.DTOs
{
    public class ProductUpdateDto
    {
        public required string Name { get; set; } 
        public required string Description { get; set; } 
        public required string FragranceType { get; set; } 
        public required string Size { get; set; } 
        public decimal Price { get; set; }
        public decimal ShippingCost { get; set; }
        public IFormFile? ImageFile { get; set; } // Optional image file
    }
}
