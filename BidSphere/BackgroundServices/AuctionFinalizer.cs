using BidSphere.Data;
using BidSphere.Models.Domain;
using BidSphere.Models.Enums;
using BidSphere.Repository.Interface;
using BidSphere.Service.Interface;
using Microsoft.EntityFrameworkCore;

namespace BidSphere.BackgroundServices
{
    /// <summary>
    /// Transitions expired auctions to payment flow by creating payment attempts and sending notifications.
    /// Runs every 10 seconds.
    /// </summary>
    public class AuctionFinalizer : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<AuctionFinalizer> _logger;

        public AuctionFinalizer(IServiceProvider serviceProvider, ILogger<AuctionFinalizer> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Auction Finalizer started - processing expired auctions every 10 seconds");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await FinalizeExpiredAuctions();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error finalizing expired auctions");
                }

                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
        }

        private async Task FinalizeExpiredAuctions()
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
            var paymentRepository = scope.ServiceProvider.GetRequiredService<IPaymentRepository>();

            // Find expired auctions that haven't been finalized yet (no payment attempts)
            var expiredAuctions = await context.Auctions
                .Include(a => a.Product)
                .Include(a => a.HighestBid)
                    .ThenInclude(b => b.Bidder)
                .Include(a => a.PaymentAttempts)
                .Where(a => a.Status == AuctionStatus.Expired && !a.PaymentAttempts.Any())
                .ToListAsync();

            if (!expiredAuctions.Any())
            {
                // Don't log when there's nothing to process - reduces log spam
                return;
            }

            _logger.LogInformation("Processing {Count} expired auctions for payment initiation", expiredAuctions.Count);

            foreach (var auction in expiredAuctions)
            {
                try
                {
                    // Only process auctions with bids
                    if (auction.HighestBid == null || auction.HighestBid.Bidder == null)
                    {
                        _logger.LogInformation("Auction {AuctionId} expired without bids, skipping payment flow",
                            auction.AuctionId);
                        continue;
                    }

                    // Create first payment attempt
                    var paymentAttempt = new PaymentAttempt
                    {
                        AuctionId = auction.AuctionId,
                        BidderId = auction.HighestBid.BidderId,
                        Status = PaymentStatus.Pending,
                        AttemptNumber = 1,
                        AttemptTime = DateTime.UtcNow
                    };

                    await paymentRepository.CreateAsync(paymentAttempt);
                    _logger.LogInformation("Created payment attempt #{AttemptNumber} for auction {AuctionId}, bidder {BidderId}",
                        paymentAttempt.AttemptNumber, auction.AuctionId, paymentAttempt.BidderId);

                    // Send email to winner
                    var winner = auction.HighestBid.Bidder;
                    var productName = auction.Product?.Name ?? "Unknown Product";
                    var finalPrice = auction.HighestBid.Amount;

                    if (!string.IsNullOrEmpty(winner.Email))
                    {
                        await emailService.SendAuctionWonNotificationAsync(
                            winner.Email,
                            winner.UserName ?? "User",
                            productName,
                            finalPrice
                        );
                        _logger.LogInformation("Auction won email sent to {Email} for auction {AuctionId}",
                            winner.Email, auction.AuctionId);
                    }
                    else
                    {
                        _logger.LogWarning("Winner {BidderId} for auction {AuctionId} has no email address",
                            winner.Id, auction.AuctionId);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to finalize auction {AuctionId}", auction.AuctionId);
                }
            }

            _logger.LogInformation("Successfully finalized {Count} expired auctions", expiredAuctions.Count);
        }
    }
}
