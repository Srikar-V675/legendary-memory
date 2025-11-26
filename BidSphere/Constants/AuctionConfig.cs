namespace BidSphere.Constants
{
    public static class AuctionConfig
    {
        public static int AntiSnipingThresholdSeconds { get; set; } = 60;
        public static int ExtensionDurationSeconds { get; set; } = 60;
        public static int PaymentTimeoutSeconds { get; set; } = 60;
        public static int MaxPaymentAttempts { get; set; } = 3;
    }
}
