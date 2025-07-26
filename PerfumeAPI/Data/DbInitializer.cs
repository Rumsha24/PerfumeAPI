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

            // Seed roles
            if (!await roleManager.RoleExistsAsync("Admin"))
                await roleManager.CreateAsync(new IdentityRole("Admin"));

            if (!await roleManager.RoleExistsAsync("Customer"))
                await roleManager.CreateAsync(new IdentityRole("Customer"));

            // Seed admin user
            if (await userManager.FindByEmailAsync("admin@noireessence.com") == null)
            {
                var admin = new User
                {
                    UserName = "admin@noireessence.com",
                    Email = "admin@noireessence.com",
                    FirstName = "Admin",
                    LastName = "User",
                    ShippingAddress = "123 Admin Street, Paris",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(admin, "Admin@123");
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(admin, "Admin");
            }

            // Only seed products if none exist
            if (!context.Products.Any())
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
                        Name = "Vintage Bloom",
                        Description = "A floral bouquet with rose, jasmine, and peony notes",
                        ImageUrl = "/images/products/vintage-bloom.jpg",
                        Price = 95.00m,
                        ShippingCost = 6.50m,
                        FragranceType = "Floral",
                        FragranceFamily = "Floral",
                        Size = "50ml",
                        StockQuantity = 40,
                        IsFeatured = false,
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
                        Name = "Citrus Zest",
                        Description = "Bright citrus explosion with bergamot, lemon, and mandarin",
                        ImageUrl = "/images/products/citrus-zest.jpg",
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
                        Name = "Vanilla Dream",
                        Description = "Creamy vanilla with tonka bean and a touch of caramel",
                        ImageUrl = "/images/products/vanilla-dream.jpg",
                        Price = 78.00m,
                        ShippingCost = 6.00m,
                        FragranceType = "Gourmand",
                        FragranceFamily = "Gourmand",
                        Size = "50ml",
                        StockQuantity = 45,
                        IsFeatured = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Product
                    {
                        Name = "Velvet Aurora",
                        Description = "A radiant blend of violet, musk, and soft amber",
                        ImageUrl = "/images/products/velvet-aurora.jpg",
                        Price = 99.00m,
                        ShippingCost = 0.00m,
                        FragranceType = "Floral Oriental",
                        FragranceFamily = "Floral Oriental",
                        Size = "100ml",
                        StockQuantity = 50,
                        IsFeatured = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Product
                    {
                        Name = "Midnight Peony",
                        Description = "Peony and blackcurrant wrapped in vanilla base",
                        ImageUrl = "/images/products/midnight-peony.jpg",
                        Price = 120.00m,
                        ShippingCost = 0.00m,
                        FragranceType = "Floral",
                        FragranceFamily = "Floral Fruity",
                        Size = "50ml",
                        StockQuantity = 30,
                        IsFeatured = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Product
                    {
                        Name = "Amber Whisper",
                        Description = "Warm amber, sandalwood, and a hint of fig",
                        ImageUrl = "/images/products/amber-whisper.jpg",
                        Price = 88.00m,
                        ShippingCost = 0.00m,
                        FragranceType = "Oriental",
                        FragranceFamily = "Oriental Woody",
                        Size = "90ml",
                        StockQuantity = 45,
                        IsFeatured = false,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Product
                    {
                        Name = "Celestial Bloom",
                        Description = "A celestial bouquet of jasmine, neroli, and white tea",
                        ImageUrl = "/images/products/celestial-bloom.jpg",
                        Price = 135.00m,
                        ShippingCost = 0.00m,
                        FragranceType = "Floral",
                        FragranceFamily = "Floral Fresh",
                        Size = "100ml",
                        StockQuantity = 25,
                        IsFeatured = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Product
                    {
                        Name = "Rosewood Reverie",
                        Description = "Rose, cedar, and a touch of citrus",
                        ImageUrl = "/images/products/rosewood-reverie.jpg",
                        Price = 105.00m,
                        ShippingCost = 0.00m,
                        FragranceType = "Woody Floral",
                        FragranceFamily = "Woody Floral",
                        Size = "50ml",
                        StockQuantity = 40,
                        IsFeatured = false,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Product
                    {
                        Name = "Golden Mirage",
                        Description = "A sparkling blend of saffron, pear, and golden woods",
                        ImageUrl = "/images/products/golden-mirage.jpg",
                        Price = 112.50m,
                        ShippingCost = 0.00m,
                        FragranceType = "Oriental Woody",
                        FragranceFamily = "Oriental Woody",
                        Size = "90ml",
                        StockQuantity = 35,
                        IsFeatured = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Product
                    {
                        Name = "Lush Serenity",
                        Description = "Green tea, bamboo, and lotus for a calming escape",
                        ImageUrl = "/images/products/lush-serenity.jpg",
                        Price = 89.00m,
                        ShippingCost = 0.00m,
                        FragranceType = "Fresh",
                        FragranceFamily = "Fresh Aromatic",
                        Size = "75ml",
                        StockQuantity = 60,
                        IsFeatured = false,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Product
                    {
                        Name = "Moonlit Jasmine",
                        Description = "Night-blooming jasmine with hints of citrus and musk",
                        ImageUrl = "/images/products/moonlit-jasmine.jpg",
                        Price = 75.00m,
                        ShippingCost = 0.00m,
                        FragranceType = "Floral",
                        FragranceFamily = "Floral Fresh",
                        Size = "80ml",
                        StockQuantity = 55,
                        IsFeatured = false,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Product
                    {
                        Name = "Saffron Veil",
                        Description = "Saffron, rose, and creamy sandalwood for a rich finish",
                        ImageUrl = "/images/products/saffron-veil.jpg",
                        Price = 120.00m,
                        ShippingCost = 0.00m,
                        FragranceType = "Oriental",
                        FragranceFamily = "Oriental Spicy",
                        Size = "50ml",
                        StockQuantity = 20,
                        IsFeatured = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Product
                    {
                        Name = "Petal Noir",
                        Description = "Dark rose, patchouli, and black pepper for a bold statement",
                        ImageUrl = "/images/products/petal-noir.jpg",
                        Price = 130.00m,
                        ShippingCost = 0.00m,
                        FragranceType = "Floral Woody",
                        FragranceFamily = "Floral Woody",
                        Size = "100ml",
                        StockQuantity = 30,
                        IsFeatured = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Product
                    {
                        Name = "Azure Muse",
                        Description = "Aquatic notes, blue lotus, and driftwood for a fresh vibe",
                        ImageUrl = "/images/products/azure-muse.jpg",
                        Price = 108.00m,
                        ShippingCost = 0.00m,
                        FragranceType = "Aquatic",
                        FragranceFamily = "Aquatic Fresh",
                        Size = "100ml",
                        StockQuantity = 40,
                        IsFeatured = false,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Product
                    {
                        Name = "Blush Ember",
                        Description = "Pink pepper, rose, and smoky woods for a warm embrace",
                        ImageUrl = "/images/products/blush-ember.jpg",
                        Price = 118.00m,
                        ShippingCost = 0.00m,
                        FragranceType = "Woody Floral",
                        FragranceFamily = "Woody Floral Spicy",
                        Size = "85ml",
                        StockQuantity = 35,
                        IsFeatured = false,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Product
                    {
                        Name = "Opal Essence",
                        Description = "Iris, white musk, and pear for a luminous signature",
                        ImageUrl = "/images/products/opal-essence.jpg",
                        Price = 95.00m,
                        ShippingCost = 0.00m,
                        FragranceType = "Floral",
                        FragranceFamily = "Floral Musky",
                        Size = "100ml",
                        StockQuantity = 50,
                        IsFeatured = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Product
                    {
                        Name = "Wild Iris",
                        Description = "Wild iris, bergamot, and vetiver for a fresh, green scent",
                        ImageUrl = "/images/products/wild-iris.jpg",
                        Price = 89.00m,
                        ShippingCost = 0.00m,
                        FragranceType = "Floral Fresh",
                        FragranceFamily = "Floral Green",
                        Size = "75ml",
                        StockQuantity = 45,
                        IsFeatured = false,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Product
                    {
                        Name = "Enchanted Fig",
                        Description = "Fig, coconut, and tonka bean for a sweet, creamy finish",
                        ImageUrl = "/images/products/enchanted-fig.jpg",
                        Price = 122.00m,
                        ShippingCost = 0.00m,
                        FragranceType = "Gourmand",
                        FragranceFamily = "Gourmand Fruity",
                        Size = "90ml",
                        StockQuantity = 30,
                        IsFeatured = true,
                        CreatedAt = DateTime.UtcNow
                    }
                };

                await context.Products.AddRangeAsync(products);
                await context.SaveChangesAsync();
            }
        }
    }
}