namespace Inventory.Core.DTOs.Categories;

public sealed class CategoryUpdateRequest
{
    public string CategoryName { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool IsActive { get; init; }
}
