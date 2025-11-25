using AutoMapper;
using BidSphere.Models.Domain;
using BidSphere.Models.Dtos.Bids;
using BidSphere.Models.Enums;
using BidSphere.Repository.Interface;
using BidSphere.Service.Interface;

namespace BidSphere.Service.Implementation
{
    public class BidService : IBidService
    {
        private readonly IBidRepository _bidRepository;
        private readonly IAuctionRepository _auctionRepository;
        private readonly IProductRepository _productRepository;
        private readonly IMapper _mapper;

        public BidService(
            IBidRepository bidRepository,
            IAuctionRepository auctionRepository,
            IProductRepository productRepository,
            IMapper mapper)
        {
            _bidRepository = bidRepository;
            _auctionRepository = auctionRepository;
            _productRepository = productRepository;
            _mapper = mapper;
        }

        public async Task<BidDto> PlaceBidAsync(int userId, PlaceBidDto bidDto)
        {
            // Get product with auction
            var product = await _productRepository.GetByIdAsync(bidDto.AuctionId);
            if (product?.Auction == null)
            {
                throw new Exception("Auction not found");
            }

            var auction = product.Auction;

            // Validate auction is active
            if (auction.Status != AuctionStatus.Active)
            {
                throw new Exception("Auction is not active");
            }

            // Validate user is not product owner
            if (product.OwnerId == userId)
            {
                throw new Exception("Cannot bid on your own product");
            }

            // Get current highest bid
            var highestBid = await _bidRepository.GetHighestBidAsync(auction.AuctionId);
            var minimumBid = highestBid?.Amount ?? product.StartingPrice;

            // Validate bid amount
            if (bidDto.Amount <= minimumBid)
            {
                throw new Exception($"Bid must be higher than current highest bid of ${minimumBid}");
            }

            // Anti-sniping: Check if bid is within last minute
            var timeRemaining = auction.ExpiryTime - DateTime.UtcNow;
            if (timeRemaining.TotalSeconds < 60 && timeRemaining.TotalSeconds > 0)
            {
                auction.ExpiryTime = auction.ExpiryTime.AddMinutes(1);
                auction.ExtensionCount++;
                await _auctionRepository.UpdateAsync(auction);
            }

            // Create bid
            var bid = new Bid
            {
                AuctionId = auction.AuctionId,
                BidderId = userId,
                Amount = bidDto.Amount,
                Timestamp = DateTime.UtcNow
            };

            var createdBid = await _bidRepository.CreateAsync(bid);

            // Update auction highest bid
            auction.HighestBidId = createdBid.BidId;
            await _auctionRepository.UpdateAsync(auction);

            // Reload bid with bidder info
            var bids = await _bidRepository.GetByAuctionIdAsync(auction.AuctionId);
            var newBid = bids.FirstOrDefault(b => b.BidId == createdBid.BidId);

            return _mapper.Map<BidDto>(newBid);
        }

        public async Task<IEnumerable<BidDto>> GetBidsByAuctionAsync(int auctionId)
        {
            var bids = await _bidRepository.GetByAuctionIdAsync(auctionId);
            return _mapper.Map<IEnumerable<BidDto>>(bids);
        }
    }
}
