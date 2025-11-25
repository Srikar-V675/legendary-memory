using BidSphere.Models.Domain;

namespace BidSphere.Repository.Interface
{
    public interface IAuctionRepository
    {
        Task<Auction> CreateAsync(Auction auction);
        Task<Auction?> GetByProductIdAsync(int productId);
        Task<Auction> UpdateAsync(Auction auction);
        Task<IEnumerable<Auction>> GetExpiredAuctionsAsync();
    }
}
