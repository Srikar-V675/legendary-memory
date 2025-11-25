using BidSphere.Models.Enums;

namespace BidSphere.Models.Domain
{
    public class Auction
    {
        public int AuctionId { get; set; }
        public int ProductId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime ExpiryTime { get; set; }
        public AuctionStatus Status { get; set; }
        public int? HighestBidId { get; set; }
        public int ExtensionCount { get; set; }

        public Product Product { get; set; } = null!;
        public Bid? HighestBid { get; set; }
        public ICollection<Bid> Bids { get; set; } = new List<Bid>();
        public ICollection<PaymentAttempt> PaymentAttempts { get; set; } = new List<PaymentAttempt>();
    }
}
