using Microsoft.EntityFrameworkCore;
using PerfumeAPI.Models;

namespace PerfumeAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Perfume> Perfumes { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Order> Orders { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Specify precision and scale for decimal properties
            modelBuilder.Entity<Order>()
                .Property(o => o.TotalAmount)
                .HasColumnType("decimal(18, 2)"); // 18 total digits, 2 decimal places

            modelBuilder.Entity<Perfume>()
                .Property(p => p.Price)
                .HasColumnType("decimal(18, 2)"); // 18 total digits, 2 decimal places

            base.OnModelCreating(modelBuilder);
        }
    }
}
