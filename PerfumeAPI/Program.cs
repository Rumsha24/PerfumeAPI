using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PerfumeAPI.Data;
using PerfumeAPI.Models.Entities;

var builder = WebApplication.CreateBuilder(args);

// 1. Add Configuration
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

// 2. Database Setup
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string not found");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

// 3. Identity Configuration
builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    options.Password.RequiredLength = 8;
    options.Password.RequireDigit = true;
    options.Password.RequireUppercase = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// 4. Configure Cookie Settings
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromDays(30);
});

// 5. Add Controllers with Views and API Explorer
builder.Services.AddControllersWithViews();
builder.Services.AddEndpointsApiExplorer();

// 6. Add HttpContextAccessor for accessing User in services
builder.Services.AddHttpContextAccessor();

// 7. Configure JSON Options for API responses
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = null;
});

var app = builder.Build();

// 8. Static Files Configuration
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        if (!app.Environment.IsDevelopment())
        {
            ctx.Context.Response.Headers.Append(
                "Cache-Control",
                "public,max-age=31536000");
        }
    }
});

// 9. Error Handling
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// 10. Endpoints
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllers();

// 11. Database Initialization
try
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    await context.Database.MigrateAsync();
    await SeedData.Initialize(context, userManager, roleManager);
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "Migration failed");
}

await app.RunAsync();

public static class SeedData
{
    public static async Task Initialize(AppDbContext context,
        UserManager<User> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        if (!await roleManager.RoleExistsAsync("Admin"))
        {
            await roleManager.CreateAsync(new IdentityRole("Admin"));
        }

        if (await userManager.FindByEmailAsync("admin@perfume.com") == null)
        {
            var admin = new User
            {
                UserName = "admin@perfume.com",
                Email = "admin@perfume.com"
            };

            var result = await userManager.CreateAsync(admin, "Admin123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(admin, "Admin");
            }
        }

        if (!context.Products.Any())
        {
            context.Products.AddRange(
                new Product
                {
                    Name = "Eau de Parfum",
                    Description = "Luxury fragrance with floral notes",
                    Price = 89.99m,
                    ShippingCost = 5.99m,
                    FragranceType = "Floral",
                    Size = "100ml",
                    IsFeatured = true
                },
                new Product
                {
                    Name = "Woody Essence",
                    Description = "Rich woody fragrance for men",
                    Price = 75.50m,
                    ShippingCost = 5.99m,
                    FragranceType = "Woody",
                    Size = "50ml"
                }
            );
            await context.SaveChangesAsync();
        }
    }
}