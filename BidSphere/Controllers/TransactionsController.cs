using BidSphere.Repository.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BidSphere.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TransactionsController : ControllerBase
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly ILogger<TransactionsController> _logger;

        public TransactionsController(IPaymentRepository paymentRepository, ILogger<TransactionsController> logger)
        {
            _paymentRepository = paymentRepository;
            _logger = logger;
        }

        /// <summary>
        /// Get transaction history (users see own, admins see all)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetTransactions()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var isAdmin = User.IsInRole("Admin");

            IEnumerable<BidSphere.Models.Domain.PaymentAttempt> payments;

            if (isAdmin)
            {
                payments = await _paymentRepository.GetAllAsync();
            }
            else
            {
                payments = await _paymentRepository.GetByUserIdAsync(userId);
            }

            var result = payments.Select(p => new
            {
                paymentId = p.PaymentId,
                productName = p.Auction?.Product?.Name ?? "Unknown",
                bidAmount = p.Auction?.HighestBid?.Amount ?? 0,
                status = p.Status.ToString(),
                attemptNumber = p.AttemptNumber,
                attemptTime = p.AttemptTime,
                confirmedAmount = p.ConfirmedAmount,
                confirmedAt = p.ConfirmedAt,
                bidderName = p.Bidder?.UserName ?? "Unknown"
            });

            return Ok(result);
        }
    }
}
