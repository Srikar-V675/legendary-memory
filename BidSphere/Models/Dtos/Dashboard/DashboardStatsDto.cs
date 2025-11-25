namespace BidSphere.Models.Dtos.Dashboard
{
    public class DashboardStatsDto
    {
        public int ActiveCount { get; set; }
        public int PendingPayment { get; set; }
        public int CompletedCount { get; set; }
        public int FailedCount { get; set; }
        public List<TopBidderDto> TopBidders { get; set; } = new List<TopBidderDto>();
    }

    public class TopBidderDto
    {
        public int UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public int TotalBids { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
