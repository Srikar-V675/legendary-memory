using BidSphere.Models.Dtos.Products;
using FluentValidation;

namespace BidSphere.Validators
{
    public class UpdateProductDtoValidator : AbstractValidator<UpdateProductDto>
    {
        public UpdateProductDtoValidator()
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
        }
    }
}
