namespace BidSphere.Models.Dtos.Bids
{
    public class BidDto
    {
        public int BidId { get; set; }
        public int AuctionId { get; set; }
        public int BidderId { get; set; }
        public string BidderEmail { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
