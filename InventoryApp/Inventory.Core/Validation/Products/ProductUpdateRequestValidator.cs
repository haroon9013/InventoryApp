using FluentValidation;
using Inventory.Core.DTOs.Products;

namespace Inventory.Core.Validation.Products;

public sealed class ProductUpdateRequestValidator : AbstractValidator<ProductUpdateRequest>
{
    public ProductUpdateRequestValidator()
    {
        RuleFor(x => x.ProductCode)
            .NotEmpty().WithMessage("Product code is required.")
            .MaximumLength(50).WithMessage("Product code cannot exceed 50 characters.");

        RuleFor(x => x.ProductName)
            .NotEmpty().WithMessage("Product name is required.")
            .MaximumLength(200).WithMessage("Product name cannot exceed 200 characters.");

        RuleFor(x => x.CategoryId)
            .GreaterThan(0).WithMessage("Category ID is required.");

        RuleFor(x => x.UnitId)
            .GreaterThan(0).WithMessage("Unit ID is required.");

        RuleFor(x => x.MinimumStock)
            .GreaterThanOrEqualTo(0m).WithMessage("Minimum stock must be greater than or equal to 0.");
    }
}
