namespace BidSphere.Models.Dtos.Products
{
    public class ProductDto
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal StartingPrice { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? ExpiryTime { get; set; }
        public decimal? HighestBidAmount { get; set; }
        public int? RemainingTimeMinutes { get; set; }
    }
}
