using BidSphere.Data;
using BidSphere.Models.Dtos.Dashboard;
using BidSphere.Models.Enums;
using BidSphere.Service.Interface;
using Microsoft.EntityFrameworkCore;

namespace BidSphere.Service.Implementation
{
    public class DashboardService : IDashboardService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DashboardService> _logger;

        public DashboardService(ApplicationDbContext context, ILogger<DashboardService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<DashboardMetricsDto> GetDashboardMetricsAsync()
        {
            try
            {
                _logger.LogInformation("Aggregating dashboard metrics");

                // Get auction counts by status
                var activeCount = await _context.Auctions.CountAsync(a => a.Status == AuctionStatus.Active);
                var expiredCount = await _context.Auctions.CountAsync(a => a.Status == AuctionStatus.Expired);
                var completedCount = await _context.Auctions.CountAsync(a => a.Status == AuctionStatus.Completed);
                var failedCount = await _context.Auctions.CountAsync(a => a.Status == AuctionStatus.Failed);

                // Get payment counts
                var pendingPaymentCount = await _context.PaymentAttempts.CountAsync(p => p.Status == PaymentStatus.Pending);
                var successfulPaymentCount = await _context.PaymentAttempts.CountAsync(p => p.Status == PaymentStatus.Success);
                var failedPaymentCount = await _context.PaymentAttempts.CountAsync(p => p.Status == PaymentStatus.Failed);

                // Calculate payment success rate
                var totalPayments = successfulPaymentCount + failedPaymentCount;
                var paymentSuccessRate = totalPayments > 0
                    ? Math.Round((double)successfulPaymentCount / totalPayments * 100, 2)
                    : 0;

                // Get top bidders
                var topBidders = await _context.Bids
                    .GroupBy(b => new { b.BidderId, b.Bidder.Email })
                    .Select(g => new TopBidderDto
                    {
                        BidderId = g.Key.BidderId,
                        BidderEmail = g.Key.Email ?? "Unknown",
                        TotalBids = g.Count(),
                        TotalAmount = g.Sum(b => b.Amount),
                        AverageBid = Math.Round(g.Average(b => b.Amount), 2)
                    })
                    .OrderByDescending(x => x.TotalBids)
                    .Take(5)
                    .ToListAsync();

                // Get recent auctions
                var recentAuctions = await _context.Auctions
                    .Include(a => a.Product)
                    .Include(a => a.Bids)
                    .OrderByDescending(a => a.StartTime)
                    .Take(5)
                    .Select(a => new RecentAuctionDto
                    {
                        AuctionId = a.AuctionId,
                        ProductName = a.Product.Name,
                        Status = a.Status.ToString(),
                        StartTime = a.StartTime,
                        ExpiryTime = a.ExpiryTime,
                        HighestBid = a.HighestBid != null ? a.HighestBid.Amount : 0,
                        BidCount = a.Bids.Count
                    })
                    .ToListAsync();

                // Get total revenue
                var totalRevenue = await _context.PaymentAttempts
                    .Where(p => p.Status == PaymentStatus.Success)
                    .SumAsync(p => p.ConfirmedAmount ?? 0);

                var metrics = new DashboardMetricsDto
                {
                    ActiveCount = activeCount,
                    ExpiredCount = expiredCount,
                    CompletedCount = completedCount,
                    FailedCount = failedCount,
                    TotalAuctions = activeCount + expiredCount + completedCount + failedCount,
                    PendingPayment = pendingPaymentCount,
                    SuccessfulPayments = successfulPaymentCount,
                    FailedPayments = failedPaymentCount,
                    PaymentSuccessRate = paymentSuccessRate,
                    TotalRevenue = Math.Round(totalRevenue, 2),
                    TopBidders = topBidders,
                    RecentAuctions = recentAuctions,
                    LastUpdated = DateTime.UtcNow
                };

                _logger.LogInformation("Dashboard metrics aggregated successfully");
                return metrics;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error aggregating dashboard metrics");
                throw;
            }
        }
    }
}
