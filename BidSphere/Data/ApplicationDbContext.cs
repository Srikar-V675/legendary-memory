#region References
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using BidSphere.Models.Domain;

#endregion

namespace BidSphere.Data
{
    public class ApplicationDbContext : IdentityDbContext<User, IdentityRole<int>, int>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Product> Products { get; set; } = null!;
        public DbSet<Auction> Auctions { get; set; } = null!;
        public DbSet<Bid> Bids { get; set; } = null!;
        public DbSet<PaymentAttempt> PaymentAttempts { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Product entity
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(p => p.ProductId);
                entity.Property(p => p.Name).IsRequired().HasMaxLength(255);
                entity.Property(p => p.Category).IsRequired().HasMaxLength(100);
                entity.Property(p => p.StartingPrice).HasColumnType("decimal(18,2)");

                entity.HasOne(p => p.Owner)
                    .WithMany(u => u.Products)
                    .HasForeignKey(p => p.OwnerId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Auction entity
            modelBuilder.Entity<Auction>(entity =>
            {
                entity.HasKey(a => a.AuctionId);

                entity.HasOne(a => a.Product)
                    .WithOne(p => p.Auction)
                    .HasForeignKey<Auction>(a => a.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(a => a.HighestBid)
                    .WithMany()
                    .HasForeignKey(a => a.HighestBidId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Bid entity
            modelBuilder.Entity<Bid>(entity =>
            {
                entity.HasKey(b => b.BidId);
                entity.Property(b => b.Amount).HasColumnType("decimal(18,2)");

                entity.HasOne(b => b.Auction)
                    .WithMany(a => a.Bids)
                    .HasForeignKey(b => b.AuctionId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(b => b.Bidder)
                    .WithMany(u => u.Bids)
                    .HasForeignKey(b => b.BidderId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // PaymentAttempt entity
            modelBuilder.Entity<PaymentAttempt>(entity =>
            {
                entity.HasKey(p => p.PaymentId);
                entity.Property(p => p.ConfirmedAmount).HasColumnType("decimal(18,2)");

                entity.HasOne(p => p.Auction)
                    .WithMany(a => a.PaymentAttempts)
                    .HasForeignKey(p => p.AuctionId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(p => p.Bidder)
                    .WithMany(u => u.PaymentAttempts)
                    .HasForeignKey(p => p.BidderId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
