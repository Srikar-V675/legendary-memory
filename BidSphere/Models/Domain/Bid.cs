using System.ComponentModel.DataAnnotations;

namespace BidSphere.Models.Domain
{
    public class Bid
    {
        public int BidId { get; set; }

        [Required]
        public int AuctionId { get; set; }

        [Required]
        public int BidderId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Bid amount must be greater than 0")]
        public decimal Amount { get; set; }

        [Required]
        public DateTime Timestamp { get; set; }

        public Auction Auction { get; set; } = null!;
        public User Bidder { get; set; } = null!;
    }
}
