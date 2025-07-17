using Microsoft.AspNetCore.Identity;
using PerfumeAPI.Models.Entities;

namespace PerfumeAPI.Data
{
    public static class DbInitializer
    {
        public static async Task Initialize(
            AppDbContext context,
            UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            await context.Database.EnsureCreatedAsync();

            // Seed roles
            if (!await roleManager.RoleExistsAsync("Admin"))
            {
                await roleManager.CreateAsync(new IdentityRole("Admin"));
            }
            if (!await roleManager.RoleExistsAsync("Customer"))
            {
                await roleManager.CreateAsync(new IdentityRole("Customer"));
            }

            // Seed admin user
            if (await userManager.FindByEmailAsync("admin@noireessence.com") == null)
            {
                var admin = new User
                {
                    UserName = "admin@noireessence.com",
                    Email = "admin@noireessence.com",
                    FirstName = "Admin",
                    LastName = "User",
                    ShippingAddress = "123 Admin Street, Paris"
                };

                var result = await userManager.CreateAsync(admin, "Admin@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, "Admin");
                }
            }

            // Seed sample products
            if (!context.Products.Any())
            {
                var products = new List<Product>
                {
                    new Product
                    {
                        Name = "Noir Essence",
                        Description = "Our signature fragrance with notes of bergamot, sandalwood, and vanilla",
                        Price = 120.00m,
                        ShippingCost = 8.00m,
                        FragranceType = "Woody",
                        Size = "100ml",
                        ImageUrl = "/images/products/noir-essence.jpg"
                    },
                    new Product
                    {
                        Name = "Vintage Bloom",
                        Description = "A floral bouquet with rose, jasmine, and peony notes",
                        Price = 95.00m,
                        ShippingCost = 6.50m,
                        FragranceType = "Floral",
                        Size = "50ml",
                        ImageUrl = "/images/products/vintage-bloom.jpg"
                    }
                };

                await context.Products.AddRangeAsync(products);
                await context.SaveChangesAsync();
            }
        }
    }
}