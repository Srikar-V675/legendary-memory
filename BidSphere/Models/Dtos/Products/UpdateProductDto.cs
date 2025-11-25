namespace BidSphere.Models.Dtos.Products
{
    public class UpdateProductDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal StartingPrice { get; set; }
    }
}
