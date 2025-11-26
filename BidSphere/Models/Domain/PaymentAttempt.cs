using BidSphere.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace BidSphere.Models.Domain
{
    public class PaymentAttempt
    {
        public int PaymentId { get; set; }

        [Required]
        public int AuctionId { get; set; }

        [Required]
        public int BidderId { get; set; }

        [Required]
        public PaymentStatus Status { get; set; }

        [Required]
        [Range(1, 3, ErrorMessage = "Attempt number must be between 1 and 3")]
        public int AttemptNumber { get; set; }

        [Required]
        public DateTime AttemptTime { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Confirmed amount must be greater than 0")]
        public decimal? ConfirmedAmount { get; set; }

        public DateTime? ConfirmedAt { get; set; }

        // Navigation properties
        public Auction Auction { get; set; } = null!;
        public User Bidder { get; set; } = null!;
    }
}
