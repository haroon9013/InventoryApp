using FluentValidation;
using Inventory.Core.DTOs.Users;

namespace Inventory.Core.Validation.Users;

public sealed class UserUpdateRequestValidator : AbstractValidator<UserUpdateRequest>
{
    public UserUpdateRequestValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full name is required.")
            .MaximumLength(150).WithMessage("Full name cannot exceed 150 characters.");

        RuleFor(x => x.UserName)
            .NotEmpty().WithMessage("Username is required.")
            .MaximumLength(100).WithMessage("Username cannot exceed 100 characters.");

        RuleFor(x => x.RoleId)
            .GreaterThan(0).WithMessage("Valid RoleId is required.");
    }
}
