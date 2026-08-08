namespace Inventory.Core.DTOs.Categories;

public sealed class CategoryCreateRequest
{
    public string CategoryName { get; init; } = string.Empty;
    public string? Description { get; init; }
}
