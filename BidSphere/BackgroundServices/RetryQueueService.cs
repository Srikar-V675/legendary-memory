using BidSphere.Constants;
using BidSphere.Models.Domain;
using BidSphere.Models.Enums;
using BidSphere.Repository.Interface;
using BidSphere.Service.Interface;

namespace BidSphere.BackgroundServices
{
    public class RetryQueueService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<RetryQueueService> _logger;

        public RetryQueueService(IServiceProvider serviceProvider, ILogger<RetryQueueService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Retry Queue Service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessTimedOutPayments();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing timed out payments");
                }

                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
        }

        private async Task ProcessTimedOutPayments()
        {
            using var scope = _serviceProvider.CreateScope();
            var paymentRepository = scope.ServiceProvider.GetRequiredService<IPaymentRepository>();
            var bidRepository = scope.ServiceProvider.GetRequiredService<IBidRepository>();
            var auctionRepository = scope.ServiceProvider.GetRequiredService<IAuctionRepository>();
            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

            var timedOutPayments = await paymentRepository.GetTimedOutPaymentsAsync();

            foreach (var payment in timedOutPayments)
            {
                _logger.LogInformation("Processing timed out payment {PaymentId} for auction {AuctionId}, attempt #{AttemptNumber}",
                    payment.PaymentId, payment.AuctionId, payment.AttemptNumber);

                // Mark current payment as failed
                payment.Status = PaymentStatus.Failed;
                await paymentRepository.UpdateAsync(payment);

                // Check if we can retry
                if (payment.AttemptNumber < AuctionConfig.MaxPaymentAttempts)
                {
                    // Get next highest bidder (excluding all previous failed bidders)
                    var failedBidderIds = (await paymentRepository.GetByAuctionIdAsync(payment.AuctionId))
                        .Select(p => p.BidderId)
                        .ToList();

                    var nextBid = await bidRepository.GetNextHighestBidderAsync(payment.AuctionId, failedBidderIds);

                    if (nextBid != null)
                    {
                        // Create new payment attempt for next bidder
                        var newPaymentAttempt = new PaymentAttempt
                        {
                            AuctionId = payment.AuctionId,
                            BidderId = nextBid.BidderId,
                            Status = PaymentStatus.Pending,
                            AttemptNumber = payment.AttemptNumber + 1,
                            AttemptTime = DateTime.UtcNow
                        };

                        await paymentRepository.CreateAsync(newPaymentAttempt);
                        _logger.LogInformation("Created payment attempt #{AttemptNumber} for auction {AuctionId}, new bidder {BidderId}",
                            newPaymentAttempt.AttemptNumber, payment.AuctionId, newPaymentAttempt.BidderId);

                        // Send email to new winner
                        try
                        {
                            if (nextBid.Bidder?.Email != null)
                            {
                                await emailService.SendAuctionWonNotificationAsync(
                                    nextBid.Bidder.Email,
                                    nextBid.Bidder.UserName ?? "User",
                                    payment.Auction?.Product?.Name ?? "Unknown Product",
                                    nextBid.Amount
                                );
                                _logger.LogInformation("Retry email sent to {Email} for auction {AuctionId}",
                                    nextBid.Bidder.Email, payment.AuctionId);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to send retry email for auction {AuctionId}", payment.AuctionId);
                        }
                    }
                    else
                    {
                        // No more bidders available
                        _logger.LogWarning("No more bidders available for auction {AuctionId}, marking as FAILED", payment.AuctionId);
                        var auction = await auctionRepository.GetByIdAsync(payment.AuctionId);
                        if (auction != null)
                        {
                            auction.Status = AuctionStatus.Failed;
                            await auctionRepository.UpdateAsync(auction);
                        }
                    }
                }
                else
                {
                    // Max attempts reached, mark auction as failed
                    _logger.LogWarning("Max payment attempts ({MaxAttempts}) reached for auction {AuctionId}, marking as FAILED",
                        AuctionConfig.MaxPaymentAttempts, payment.AuctionId);

                    var auction = await auctionRepository.GetByIdAsync(payment.AuctionId);
                    if (auction != null)
                    {
                        auction.Status = AuctionStatus.Failed;
                        await auctionRepository.UpdateAsync(auction);
                    }
                }
            }
        }
    }
}
