using BidSphere.Models.Dtos.Products;
using FluentValidation;

namespace BidSphere.Validators
{
    public class ConfirmPaymentDtoValidator : AbstractValidator<ConfirmPaymentDto>
    {
        public ConfirmPaymentDtoValidator()
        {
            RuleFor(x => x.ConfirmedAmount)
                .GreaterThan(0)
                .WithMessage("Confirmed amount must be greater than 0")
                .LessThanOrEqualTo(1000000)
                .WithMessage("Confirmed amount cannot exceed $1,000,000");
        }
    }
}
