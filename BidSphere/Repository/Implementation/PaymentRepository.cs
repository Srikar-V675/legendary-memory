using BidSphere.Data;
using BidSphere.Models.Domain;
using BidSphere.Models.Enums;
using BidSphere.Repository.Interface;
using BidSphere.Constants;
using Microsoft.EntityFrameworkCore;

namespace BidSphere.Repository.Implementation
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly ApplicationDbContext _context;

        public PaymentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PaymentAttempt> CreateAsync(PaymentAttempt paymentAttempt)
        {
            _context.PaymentAttempts.Add(paymentAttempt);
            await _context.SaveChangesAsync();
            return paymentAttempt;
        }

        public async Task<IEnumerable<PaymentAttempt>> GetByAuctionIdAsync(int auctionId)
        {
            return await _context.PaymentAttempts
                .Include(p => p.Bidder)
                .Include(p => p.Auction)
                    .ThenInclude(a => a.Product)
                .Where(p => p.AuctionId == auctionId)
                .OrderBy(p => p.AttemptNumber)
                .ToListAsync();
        }

        public async Task<PaymentAttempt?> GetPendingPaymentAsync(int auctionId)
        {
            return await _context.PaymentAttempts
                .Include(p => p.Bidder)
                .Include(p => p.Auction)
                .FirstOrDefaultAsync(p => p.AuctionId == auctionId && p.Status == PaymentStatus.Pending);
        }

        public async Task<IEnumerable<PaymentAttempt>> GetTimedOutPaymentsAsync()
        {
            var timeoutThreshold = DateTime.UtcNow.AddSeconds(-AuctionConfig.PaymentTimeoutSeconds);

            return await _context.PaymentAttempts
                .Include(p => p.Auction)
                    .ThenInclude(a => a.Product)
                .Include(p => p.Bidder)
                .Where(p => p.Status == PaymentStatus.Pending && p.AttemptTime <= timeoutThreshold)
                .ToListAsync();
        }

        public async Task UpdateAsync(PaymentAttempt paymentAttempt)
        {
            _context.PaymentAttempts.Update(paymentAttempt);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<PaymentAttempt>> GetByUserIdAsync(int userId)
        {
            return await _context.PaymentAttempts
                .Include(p => p.Bidder)
                .Include(p => p.Auction)
                    .ThenInclude(a => a.Product)
                .Include(p => p.Auction)
                    .ThenInclude(a => a.Bids)
                .Where(p => p.BidderId == userId)
                .OrderByDescending(p => p.AttemptTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<PaymentAttempt>> GetAllAsync()
        {
            return await _context.PaymentAttempts
                .Include(p => p.Bidder)
                .Include(p => p.Auction)
                    .ThenInclude(a => a.Product)
                .Include(p => p.Auction)
                    .ThenInclude(a => a.Bids)
                .OrderByDescending(p => p.AttemptTime)
                .ToListAsync();
        }
    }
}
