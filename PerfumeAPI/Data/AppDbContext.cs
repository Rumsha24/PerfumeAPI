using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PerfumeAPI.Models.Entities;
using System;
using System.Linq;

namespace PerfumeAPI.Data
{
    public class AppDbContext : IdentityDbContext<User>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Product> Products { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<CommentImage> CommentImages { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User-Cart relationship (1-to-1)
            modelBuilder.Entity<User>()
                .HasOne(u => u.Cart)
                .WithOne(c => c.User)
                .HasForeignKey<Cart>(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Comment-CommentImages relationship (1-to-many)
            modelBuilder.Entity<Comment>()
                .HasMany(c => c.Images)
                .WithOne(ci => ci.Comment)
                .HasForeignKey(ci => ci.CommentId)
                .OnDelete(DeleteBehavior.Cascade);

            // Cart-CartItems relationship (1-to-many)
            modelBuilder.Entity<Cart>()
                .HasMany(c => c.Items)
                .WithOne(ci => ci.Cart)
                .HasForeignKey(ci => ci.CartId)
                .OnDelete(DeleteBehavior.Cascade);

            // Product-CartItems relationship (1-to-many)
            modelBuilder.Entity<Product>()
                .HasMany(p => p.CartItems)
                .WithOne(ci => ci.Product)
                .HasForeignKey(ci => ci.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // Order-OrderItems relationship (1-to-many)
            modelBuilder.Entity<Order>()
                .HasMany(o => o.Items)
                .WithOne(oi => oi.Order)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // Product-Comments relationship (1-to-many)
            modelBuilder.Entity<Product>()
                .HasMany(p => p.Comments)
                .WithOne(c => c.Product)
                .HasForeignKey(c => c.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // Decimal precision configuration
            modelBuilder.Entity<Product>()
                .Property(p => p.Price)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Product>()
                .Property(p => p.ShippingCost)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Order>()
                .Property(o => o.TotalAmount)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<OrderItem>()
                .Property(oi => oi.PriceAtPurchase)
                .HasColumnType("decimal(18,2)");

            // CartItem timestamps
            modelBuilder.Entity<CartItem>()
                .Property(ci => ci.AddedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            modelBuilder.Entity<CartItem>()
                .Property(ci => ci.UpdatedAt)
                .IsRequired(false);

            // Seed initial data if database is empty
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Seed sample products
            modelBuilder.Entity<Product>().HasData(
                new Product
                {
                    Id = 1,
                    Name = "Noire Signature",
                    Description = "Our flagship fragrance with notes of bergamot and sandalwood",
                    Price = 129.99m,
                    IsFeatured = true,
                    ImageUrl = "/images/noire-signature.jpg",
                    CreatedAt = DateTime.UtcNow,
                    StockQuantity = 100,
                    FragranceFamily = "Woody"
                },
                new Product
                {
                    Id = 2,
                    Name = "Essence Royale",
                    Description = "Luxury floral bouquet with jasmine and rose",
                    Price = 159.99m,
                    IsFeatured = true,
                    ImageUrl = "/images/essence-royale.jpg",
                    CreatedAt = DateTime.UtcNow.AddDays(-1),
                    StockQuantity = 75,
                    FragranceFamily = "Floral"
                },
                new Product
                {
                    Id = 3,
                    Name = "Citrus Splash",
                    Description = "Fresh citrus fragrance perfect for summer",
                    Price = 89.99m,
                    IsFeatured = false,
                    ImageUrl = "/images/citrus-splash.jpg",
                    CreatedAt = DateTime.UtcNow.AddDays(-2),
                    StockQuantity = 120,
                    FragranceFamily = "Fresh"
                }
            );

            // Seed sample comments
            modelBuilder.Entity<Comment>().HasData(
                new Comment
                {
                    Id = 1,
                    ProductId = 1,
                    UserId = "1", // This should match an actual user ID
                    Rating = 5,
                    Text = "Absolutely divine! The scent lasts all day.",
                    CreatedAt = DateTime.UtcNow.AddDays(-3)
                },
                new Comment
                {
                    Id = 2,
                    ProductId = 1,
                    UserId = "2",
                    Rating = 4,
                    Text = "Beautiful fragrance, but a bit strong for my taste",
                    CreatedAt = DateTime.UtcNow.AddDays(-5)
                },
                new Comment
                {
                    Id = 3,
                    ProductId = 2,
                    UserId = "1",
                    Rating = 5,
                    Text = "My new favorite! The floral notes are perfect",
                    CreatedAt = DateTime.UtcNow.AddDays(-2)
                }
            );
        }
    }
}