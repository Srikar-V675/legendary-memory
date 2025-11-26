namespace BidSphere.Exceptions
{
    public class AuctionNotFoundException : Exception
    {
        public AuctionNotFoundException() : base("Auction not found")
        {
        }

        public AuctionNotFoundException(int auctionId) : base($"Auction with ID {auctionId} not found")
        {
        }

        public AuctionNotFoundException(string message) : base(message)
        {
        }
    }
}
