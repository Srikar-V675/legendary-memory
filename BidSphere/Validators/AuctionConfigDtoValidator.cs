using BidSphere.Models.Dtos.Config;
using FluentValidation;

namespace BidSphere.Validators
{
    public class AuctionConfigDtoValidator : AbstractValidator<AuctionConfigDto>
    {
        public AuctionConfigDtoValidator()
        {
            RuleFor(x => x.AntiSnipingThresholdSeconds)
                .GreaterThanOrEqualTo(10)
                .WithMessage("Anti-sniping threshold must be at least 10 seconds")
                .LessThanOrEqualTo(300)
                .WithMessage("Anti-sniping threshold cannot exceed 5 minutes (300 seconds)");

            RuleFor(x => x.ExtensionDurationSeconds)
                .GreaterThanOrEqualTo(10)
                .WithMessage("Extension duration must be at least 10 seconds")
                .LessThanOrEqualTo(600)
                .WithMessage("Extension duration cannot exceed 10 minutes (600 seconds)");

            RuleFor(x => x.PaymentTimeoutSeconds)
                .GreaterThanOrEqualTo(30)
                .WithMessage("Payment timeout must be at least 30 seconds")
                .LessThanOrEqualTo(300)
                .WithMessage("Payment timeout cannot exceed 5 minutes (300 seconds)");

            RuleFor(x => x.MaxPaymentAttempts)
                .GreaterThanOrEqualTo(1)
                .WithMessage("Max payment attempts must be at least 1")
                .LessThanOrEqualTo(5)
                .WithMessage("Max payment attempts cannot exceed 5");
        }
    }
}
