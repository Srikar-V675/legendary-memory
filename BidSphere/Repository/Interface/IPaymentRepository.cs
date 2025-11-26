using BidSphere.Models.Domain;

namespace BidSphere.Repository.Interface
{
    public interface IPaymentRepository
    {
        Task<PaymentAttempt> CreateAsync(PaymentAttempt paymentAttempt);
        Task<IEnumerable<PaymentAttempt>> GetByAuctionIdAsync(int auctionId);
        Task<PaymentAttempt?> GetPendingPaymentAsync(int auctionId);
        Task<IEnumerable<PaymentAttempt>> GetTimedOutPaymentsAsync();
        Task UpdateAsync(PaymentAttempt paymentAttempt);
        Task<IEnumerable<PaymentAttempt>> GetByUserIdAsync(int userId);
        Task<IEnumerable<PaymentAttempt>> GetAllAsync();
    }
}
