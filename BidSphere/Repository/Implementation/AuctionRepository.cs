using BidSphere.Data;
using BidSphere.Models.Domain;
using BidSphere.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace BidSphere.Repository.Implementation
{
    public class AuctionRepository : IAuctionRepository
    {
        private readonly ApplicationDbContext _context;

        public AuctionRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Auction> CreateAsync(Auction auction)
        {
            _context.Auctions.Add(auction);
            await _context.SaveChangesAsync();
            return auction;
        }

        public async Task<Auction?> GetByProductIdAsync(int productId)
        {
            return await _context.Auctions
                .FirstOrDefaultAsync(a => a.ProductId == productId);
        }

        public async Task<Auction> UpdateAsync(Auction auction)
        {
            _context.Auctions.Update(auction);
            await _context.SaveChangesAsync();
            return auction;
        }
    }
}
