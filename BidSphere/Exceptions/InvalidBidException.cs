namespace BidSphere.Exceptions
{
    public class InvalidBidException : Exception
    {
        public InvalidBidException() : base("Invalid bid")
        {
        }

        public InvalidBidException(string message) : base(message)
        {
        }
    }
}
