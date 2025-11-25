using BidSphere.Models.Dtos.Auth;
using FluentValidation;

namespace BidSphere.Validators
{
    public class RegisterDtoValidator : AbstractValidator<RegisterDto>
    {
        public RegisterDtoValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email format");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required")
                .MinimumLength(6).WithMessage("Password must be at least 6 characters")
                .Matches(@"\d").WithMessage("Password must contain at least one digit");

            RuleFor(x => x.Role)
                .Must(role => string.IsNullOrEmpty(role) ||
                             role.Equals("Admin", StringComparison.OrdinalIgnoreCase) ||
                             role.Equals("User", StringComparison.OrdinalIgnoreCase) ||
                             role.Equals("Guest", StringComparison.OrdinalIgnoreCase))
                .WithMessage("Role must be Admin, User, or Guest");
        }
    }
}
