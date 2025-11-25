using BidSphere.Models.Domain;

namespace BidSphere.Repository.Interface
{
    public interface IBidRepository
    {
        Task<Bid> CreateAsync(Bid bid);
        Task<IEnumerable<Bid>> GetByAuctionIdAsync(int auctionId);
        Task<Bid?> GetHighestBidAsync(int auctionId);
        Task<Bid?> GetNextHighestBidderAsync(int auctionId, int excludeBidderId);
    }
}
