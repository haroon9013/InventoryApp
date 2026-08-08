using FluentValidation;
using Inventory.Core.DTOs.Units;

namespace Inventory.Core.Validation.Units;

public sealed class UnitUpdateRequestValidator : AbstractValidator<UnitUpdateRequest>
{
    public UnitUpdateRequestValidator()
    {
        RuleFor(x => x.UnitName)
            .NotEmpty().WithMessage("Unit name is required.")
            .MaximumLength(50).WithMessage("Unit name cannot exceed 50 characters.");

        RuleFor(x => x.ShortName)
            .NotEmpty().WithMessage("Short name is required.")
            .MaximumLength(20).WithMessage("Short name cannot exceed 20 characters.");
    }
}
