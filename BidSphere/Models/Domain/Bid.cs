namespace BidSphere.Models.Domain
{
    public class Bid
    {
        public int BidId { get; set; }
        public int AuctionId { get; set; }
        public int BidderId { get; set; }
        public decimal Amount { get; set; }
        public DateTime Timestamp { get; set; }

        public Auction Auction { get; set; } = null!;
        public User Bidder { get; set; } = null!;
    }
}
