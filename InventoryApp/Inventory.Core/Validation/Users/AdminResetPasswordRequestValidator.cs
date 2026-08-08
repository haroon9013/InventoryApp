using FluentValidation;
using Inventory.Core.DTOs.Users;

namespace Inventory.Core.Validation.Users;

public sealed class AdminResetPasswordRequestValidator : AbstractValidator<AdminResetPasswordRequest>
{
    public AdminResetPasswordRequestValidator()
    {
        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("New password is required.")
            .MinimumLength(6).WithMessage("New password must be at least 6 characters long.");
    }
}
