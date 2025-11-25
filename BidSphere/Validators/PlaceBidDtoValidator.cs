using BidSphere.Models.Dtos.Bids;
using FluentValidation;

namespace BidSphere.Validators
{
    public class PlaceBidDtoValidator : AbstractValidator<PlaceBidDto>
    {
        public PlaceBidDtoValidator()
        {
            RuleFor(x => x.AuctionId)
                .GreaterThan(0).WithMessage("Auction ID must be greater than 0");

            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage("Bid amount must be greater than 0");
        }
    }
}
