namespace BidSphere.Models.Dtos.Payments
{
    public class TransactionDto
    {
        public int PaymentId { get; set; }
        public int AuctionId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int BidderId { get; set; }
        public string BidderEmail { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int AttemptNumber { get; set; }
        public DateTime AttemptTime { get; set; }
        public decimal? ConfirmedAmount { get; set; }
    }
}
