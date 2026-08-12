namespace Inventory.Core.DTOs.Products;

public sealed class ProductResponse
{
    public int Id { get; init; }
    public string ProductCode { get; init; } = string.Empty;
    public string ProductName { get; init; } = string.Empty;
    public int CategoryId { get; init; }
    public string CategoryName { get; init; } = string.Empty;
    public int UnitId { get; init; }
    public string UnitName { get; init; } = string.Empty;
    public decimal CurrentStock { get; init; }
    public decimal MinimumStock { get; init; }
    public decimal LastPurchasePrice { get; init; }
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}
