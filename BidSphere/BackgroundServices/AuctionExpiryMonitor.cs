using BidSphere.Data;
using BidSphere.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace BidSphere.BackgroundServices
{
    /// <summary>
    /// Monitors active auctions and marks them as expired when their expiry time is reached.
    /// Runs every 10 seconds.
    /// </summary>
    public class AuctionExpiryMonitor : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<AuctionExpiryMonitor> _logger;

        public AuctionExpiryMonitor(IServiceProvider serviceProvider, ILogger<AuctionExpiryMonitor> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Auction Expiry Monitor started - checking every 10 seconds");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CheckExpiredAuctions();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error checking expired auctions");
                }

                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
        }

        private async Task CheckExpiredAuctions()
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var expiredAuctions = await context.Auctions
                .Where(a => a.Status == AuctionStatus.Active && a.ExpiryTime <= DateTime.UtcNow)
                .ToListAsync();

            if (!expiredAuctions.Any())
            {
                return;
            }

            foreach (var auction in expiredAuctions)
            {
                auction.Status = AuctionStatus.Expired;
                _logger.LogInformation("Auction {AuctionId} marked as EXPIRED at {ExpiryTime}",
                    auction.AuctionId, auction.ExpiryTime);
            }

            await context.SaveChangesAsync();
            _logger.LogInformation("Marked {Count} auctions as expired", expiredAuctions.Count);
        }
    }
}
