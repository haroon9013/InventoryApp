namespace Inventory.Core.DTOs.Products;

public sealed class ProductCreateRequest
{
    public string ProductCode { get; init; } = string.Empty;
    public string ProductName { get; init; } = string.Empty;
    public int CategoryId { get; init; }
    public int UnitId { get; init; }
    public decimal MinimumStock { get; init; }
}
