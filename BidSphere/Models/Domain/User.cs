using BidSphere.Models.Enums;
using Microsoft.AspNetCore.Identity;

namespace BidSphere.Models.Domain
{
    public class User : IdentityUser<int>
    {
        public UserRole Role { get; set; }
        public DateTime CreatedAt { get; set; }

        public ICollection<Product> Products { get; set; } = new List<Product>();
        public ICollection<Bid> Bids { get; set; } = new List<Bid>();
        public ICollection<PaymentAttempt> PaymentAttempts { get; set; } = new List<PaymentAttempt>();
    }
}
