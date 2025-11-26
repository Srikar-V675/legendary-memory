using System.ComponentModel.DataAnnotations;

namespace BidSphere.Models.Domain
{
    public class Product
    {
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Product name is required")]
        [StringLength(255, MinimumLength = 3, ErrorMessage = "Product name must be between 3 and 255 characters")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required")]
        [StringLength(2000, MinimumLength = 10, ErrorMessage = "Description must be between 10 and 2000 characters")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Category is required")]
        [StringLength(100, ErrorMessage = "Category cannot exceed 100 characters")]
        public string Category { get; set; } = string.Empty;

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Starting price must be greater than 0")]
        public decimal StartingPrice { get; set; }

        [Required]
        [Range(2, 1440, ErrorMessage = "Auction duration must be between 2 minutes and 24 hours (1440 minutes)")]
        public int AuctionDurationMinutes { get; set; }

        [Required]
        public int OwnerId { get; set; }

        public DateTime CreatedAt { get; set; }

        public User Owner { get; set; } = null!;
        public Auction? Auction { get; set; }
    }
}
