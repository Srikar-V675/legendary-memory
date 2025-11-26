namespace BidSphere.Exceptions
{
    public class UnauthorizedBidException : Exception
    {
        public UnauthorizedBidException() : base("Unauthorized to place bid")
        {
        }

        public UnauthorizedBidException(string message) : base(message)
        {
        }
    }
}
