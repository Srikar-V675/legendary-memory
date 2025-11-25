using BidSphere.Models.Dtos.Bids;

namespace BidSphere.Service.Interface
{
    public interface IBidService
    {
        Task<BidDto> PlaceBidAsync(int userId, PlaceBidDto bidDto);
        Task<IEnumerable<BidDto>> GetBidsByAuctionAsync(int auctionId);
    }
}
