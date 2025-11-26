using BidSphere.Data;
using BidSphere.Models.Domain;
using BidSphere.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace BidSphere.Repository.Implementation
{
    public class BidRepository : IBidRepository
    {
        private readonly ApplicationDbContext _context;

        public BidRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Bid> CreateAsync(Bid bid)
        {
            _context.Bids.Add(bid);
            await _context.SaveChangesAsync();
            return bid;
        }

        public async Task<IEnumerable<Bid>> GetByAuctionIdAsync(int auctionId)
        {
            return await _context.Bids
                .Include(b => b.Bidder)
                .Where(b => b.AuctionId == auctionId)
                .OrderByDescending(b => b.Timestamp)
                .ToListAsync();
        }

        public async Task<Bid?> GetHighestBidAsync(int auctionId)
        {
            return await _context.Bids
                .Where(b => b.AuctionId == auctionId)
                .OrderByDescending(b => b.Amount)
                .FirstOrDefaultAsync();
        }

        public async Task<Bid?> GetNextHighestBidderAsync(int auctionId, int excludeBidderId)
        {
            return await _context.Bids
                .Where(b => b.AuctionId == auctionId && b.BidderId != excludeBidderId)
                .OrderByDescending(b => b.Amount)
                .FirstOrDefaultAsync();
        }

        public async Task<Bid?> GetNextHighestBidderAsync(int auctionId, List<int> excludeBidderIds)
        {
            return await _context.Bids
                .Include(b => b.Bidder)
                .Where(b => b.AuctionId == auctionId && !excludeBidderIds.Contains(b.BidderId))
                .OrderByDescending(b => b.Amount)
                .FirstOrDefaultAsync();
        }
    }
}
