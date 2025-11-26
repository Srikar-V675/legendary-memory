namespace BidSphere.Exceptions
{
    public class ProductNotFoundException : Exception
    {
        public ProductNotFoundException() : base("Product not found")
        {
        }

        public ProductNotFoundException(int productId) : base($"Product with ID {productId} not found")
        {
        }

        public ProductNotFoundException(string message) : base(message)
        {
        }
    }
}
