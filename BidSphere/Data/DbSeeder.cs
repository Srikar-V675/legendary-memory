using BidSphere.Models.Domain;
using BidSphere.Models.Enums;
using Microsoft.AspNetCore.Identity;

namespace BidSphere.Data
{
    public static class DbSeeder
    {
        public static async Task SeedDataAsync(IServiceProvider serviceProvider)
        {
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = serviceProvider.GetRequiredService<UserManager<User>>();

            // Check if data already exists
            if (context.Products.Any())
            {
                return; // Data already seeded
            }

            // Create admin user
            var admin = await userManager.FindByEmailAsync("admin@bidsphere.com");
            if (admin == null)
            {
                admin = new User
                {
                    UserName = "admin@bidsphere.com",
                    Email = "admin@bidsphere.com",
                    Role = UserRole.Admin,
                    CreatedAt = DateTime.UtcNow
                };
                await userManager.CreateAsync(admin, "Admin@123");
                await userManager.AddToRoleAsync(admin, "Admin");
            }

            // Create sample products
            var products = new List<Product>
            {
                new Product
                {
                    Name = "Vintage Rolex Watch",
                    Description = "Rare 1960s Rolex Submariner in excellent condition",
                    Category = "Fashion",
                    StartingPrice = 5000,
                    AuctionDurationMinutes = 120,
                    OwnerId = admin.Id,
                    CreatedAt = DateTime.UtcNow
                },
                new Product
                {
                    Name = "MacBook Pro 16-inch",
                    Description = "2023 M3 Max, 64GB RAM, 2TB SSD",
                    Category = "Electronics",
                    StartingPrice = 2500,
                    AuctionDurationMinutes = 180,
                    OwnerId = admin.Id,
                    CreatedAt = DateTime.UtcNow
                },
                new Product
                {
                    Name = "Original Picasso Sketch",
                    Description = "Authenticated 1950s sketch by Pablo Picasso",
                    Category = "Art",
                    StartingPrice = 15000,
                    AuctionDurationMinutes = 240,
                    OwnerId = admin.Id,
                    CreatedAt = DateTime.UtcNow
                },
                new Product
                {
                    Name = "Gaming PC Setup",
                    Description = "RTX 4090, i9-13900K, 64GB RAM, complete setup",
                    Category = "Electronics",
                    StartingPrice = 3000,
                    AuctionDurationMinutes = 90,
                    OwnerId = admin.Id,
                    CreatedAt = DateTime.UtcNow
                },
                new Product
                {
                    Name = "Antique Persian Rug",
                    Description = "Hand-woven 19th century Persian rug, 8x10 feet",
                    Category = "Art",
                    StartingPrice = 8000,
                    AuctionDurationMinutes = 300,
                    OwnerId = admin.Id,
                    CreatedAt = DateTime.UtcNow
                }
            };

            context.Products.AddRange(products);
            await context.SaveChangesAsync();

            // Create auctions for each product
            var auctions = new List<Auction>();
            foreach (var product in products)
            {
                auctions.Add(new Auction
                {
                    ProductId = product.ProductId,
                    StartTime = DateTime.UtcNow,
                    ExpiryTime = DateTime.UtcNow.AddMinutes(product.AuctionDurationMinutes),
                    Status = AuctionStatus.Active,
                    ExtensionCount = 0
                });
            }

            context.Auctions.AddRange(auctions);
            await context.SaveChangesAsync();
        }
    }
}
