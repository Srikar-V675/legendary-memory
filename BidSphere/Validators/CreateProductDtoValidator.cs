using BidSphere.Models.Dtos.Products;
using FluentValidation;

namespace BidSphere.Validators
{
    public class CreateProductDtoValidator : AbstractValidator<CreateProductDto>
    {
        public CreateProductDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Product name is required")
                .MaximumLength(255).WithMessage("Product name cannot exceed 255 characters");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Description is required");

            RuleFor(x => x.Category)
                .NotEmpty().WithMessage("Category is required")
                .MaximumLength(100).WithMessage("Category cannot exceed 100 characters");

            RuleFor(x => x.StartingPrice)
                .GreaterThan(0).WithMessage("Starting price must be greater than 0");

            RuleFor(x => x.AuctionDurationMinutes)
                .InclusiveBetween(2, 1440).WithMessage("Auction duration must be between 2 minutes and 24 hours (1440 minutes)");
        }
    }
}
