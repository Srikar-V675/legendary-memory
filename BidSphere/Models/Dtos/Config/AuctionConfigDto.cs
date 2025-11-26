namespace BidSphere.Models.Dtos.Config
{
    public class AuctionConfigDto
    {
        public int AntiSnipingThresholdSeconds { get; set; }
        public int ExtensionDurationSeconds { get; set; }
        public int PaymentTimeoutSeconds { get; set; }
        public int MaxPaymentAttempts { get; set; }
    }
}
