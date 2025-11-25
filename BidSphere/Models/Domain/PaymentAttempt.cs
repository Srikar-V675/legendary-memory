using BidSphere.Models.Enums;

namespace BidSphere.Models.Domain
{
    public class PaymentAttempt
    {
        public int PaymentId { get; set; }
        public int AuctionId { get; set; }
        public int BidderId { get; set; }
        public PaymentStatus Status { get; set; }
        public int AttemptNumber { get; set; }
        public DateTime AttemptTime { get; set; }
        public decimal? ConfirmedAmount { get; set; }

        public Auction Auction { get; set; } = null!;
        public User Bidder { get; set; } = null!;
    }
}
