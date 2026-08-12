using FluentValidation;
using Inventory.Core.DTOs.Departments;

namespace Inventory.Core.Validation.Departments;

public sealed class DepartmentUpdateRequestValidator : AbstractValidator<DepartmentUpdateRequest>
{
    public DepartmentUpdateRequestValidator()
    {
        RuleFor(x => x.DepartmentName)
            .NotEmpty().WithMessage("Department name is required.")
            .MaximumLength(100).WithMessage("Department name cannot exceed 100 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters.");
    }
}
