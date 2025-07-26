using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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
            // Ensure database is created and migrated
            await context.Database.MigrateAsync();

            // Seed roles and admin user first
            await SeedRolesAndAdmin(userManager, roleManager);

            // Only seed products if none exist
            if (!await context.Products.AnyAsync())
            {
                await SeedProducts(context);
            }

            // Seed sample comments if none exist
            if (!await context.Comments.AnyAsync())
            {
                await SeedComments(context);
            }
        }

        private static async Task SeedRolesAndAdmin(
            UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            // Seed roles
            string[] roles = { "Admin", "Customer" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // Seed admin user
            const string adminEmail = "admin@noireessence.com";
            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var admin = new User
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FirstName = "Admin",
                    LastName = "User",
                    ShippingAddress = "123 Admin Street, Paris",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(admin, "Admin@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, "Admin");
                }
            }
        }

        private static async Task SeedProducts(AppDbContext context)
        {
            var products = new List<Product>
            {
                new Product
                {
                    Name = "Noir Essence",
                    Description = "Our signature fragrance with notes of bergamot, sandalwood, and vanilla",
                    ImageUrl = "/images/products/noir-essence.jpg",
                    Price = 120.00m,
                    ShippingCost = 8.00m,
                    FragranceType = "Woody",
                    FragranceFamily = "Woody Oriental",
                    Size = "100ml",
                    StockQuantity = 50,
                    IsFeatured = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Product
                {
                    Name = "Citrus Splash",
                    Description = "Bright citrus explosion with bergamot, lemon, and mandarin",
                    ImageUrl = "/images/products/citrus-splash.jpg",
                    Price = 85.00m,
                    ShippingCost = 5.50m,
                    FragranceType = "Citrus",
                    FragranceFamily = "Citrus",
                    Size = "75ml",
                    StockQuantity = 60,
                    IsFeatured = false,
                    CreatedAt = DateTime.UtcNow
                },
                new Product
                {
                    Name = "Velvet Rose",
                    Description = "Luxurious rose petals with hints of vanilla and amber",
                    ImageUrl = "/images/products/velvet-rose.jpg",
                    Price = 95.00m,
                    ShippingCost = 7.00m,
                    FragranceType = "Floral",
                    FragranceFamily = "Floral Oriental",
                    Size = "50ml",
                    StockQuantity = 40,
                    IsFeatured = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Product
                {
                    Name = "Mystic Woods",
                    Description = "Deep forest notes with cedar, vetiver, and a hint of smoke",
                    ImageUrl = "/images/products/mystic-woods.jpg",
                    Price = 110.00m,
                    ShippingCost = 7.00m,
                    FragranceType = "Woody",
                    FragranceFamily = "Woody Aromatic",
                    Size = "100ml",
                    StockQuantity = 35,
                    IsFeatured = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Product
                {
                    Name = "Vanilla Dream",
                    Description = "Creamy vanilla with tonka bean and a touch of caramel",
                    ImageUrl = "/images/products/vanilla-dream.jpg",
                    Price = 78.00m,
                    ShippingCost = 6.00m,
                    FragranceType = "Gourmand",
                    FragranceFamily = "Gourmand",
                    Size = "50ml",
                    StockQuantity = 45,
                    IsFeatured = false,
                    CreatedAt = DateTime.UtcNow
                }
            };

            await context.Products.AddRangeAsync(products);
            await context.SaveChangesAsync();
        }

        private static async Task SeedComments(AppDbContext context)
        {
            // Get the first admin user
            var admin = await context.Users.FirstOrDefaultAsync(u => u.Email == "admin@noireessence.com");
            if (admin == null) return;

            // Get some products
            var products = await context.Products.Take(3).ToListAsync();
            if (!products.Any()) return;

            var comments = new List<Comment>
            {
                new Comment
                {
                    ProductId = products[0].Id,
                    UserId = admin.Id,
                    Rating = 5,
                    Text = "Absolutely love this fragrance! The sandalwood notes are perfect.",
                    CreatedAt = DateTime.UtcNow.AddDays(-3)
                },
                new Comment
                {
                    ProductId = products[0].Id,
                    UserId = admin.Id,
                    Rating = 4,
                    Text = "Great longevity, though a bit strong for my taste.",
                    CreatedAt = DateTime.UtcNow.AddDays(-5)
                },
                new Comment
                {
                    ProductId = products[1].Id,
                    UserId = admin.Id,
                    Rating = 5,
                    Text = "Perfect summer fragrance! So refreshing.",
                    CreatedAt = DateTime.UtcNow.AddDays(-2)
                },
                new Comment
                {
                    ProductId = products[2].Id,
                    UserId = admin.Id,
                    Rating = 4,
                    Text = "Beautiful floral scent, lasts all day.",
                    CreatedAt = DateTime.UtcNow.AddDays(-1)
                }
            };

            await context.Comments.AddRangeAsync(comments);
            await context.SaveChangesAsync();
        }
    }
}