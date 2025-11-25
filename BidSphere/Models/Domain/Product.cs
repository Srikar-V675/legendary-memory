namespace BidSphere.Models.Domain
{
    public class Product
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal StartingPrice { get; set; }
        public int AuctionDurationMinutes { get; set; }
        public int OwnerId { get; set; }
        public DateTime CreatedAt { get; set; }

        public User Owner { get; set; } = null!;
        public Auction? Auction { get; set; }
    }
}
