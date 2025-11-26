namespace BidSphere.Exceptions
{
    public class PaymentException : Exception
    {
        public PaymentException() : base("Payment error occurred")
        {
        }

        public PaymentException(string message) : base(message)
        {
        }
    }
}
