namespace BidSphere.Models.Dtos.Dashboard
{
    public class DashboardMetricsDto
    {
        public int ActiveCount { get; set; }
        public int ExpiredCount { get; set; }
        public int CompletedCount { get; set; }
        public int FailedCount { get; set; }
        public int TotalAuctions { get; set; }
        public int PendingPayment { get; set; }
        public int SuccessfulPayments { get; set; }
        public int FailedPayments { get; set; }
        public double PaymentSuccessRate { get; set; }
        public decimal TotalRevenue { get; set; }
        public List<TopBidderDto> TopBidders { get; set; } = new();
        public List<RecentAuctionDto> RecentAuctions { get; set; } = new();
        public DateTime LastUpdated { get; set; }
    }

    public class TopBidderDto
    {
        public int BidderId { get; set; }
        public string BidderEmail { get; set; } = string.Empty;
        public int TotalBids { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal AverageBid { get; set; }
    }

    public class RecentAuctionDto
    {
        public int AuctionId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime ExpiryTime { get; set; }
        public decimal HighestBid { get; set; }
        public int BidCount { get; set; }
    }
}
