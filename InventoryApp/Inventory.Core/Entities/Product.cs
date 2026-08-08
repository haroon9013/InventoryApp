namespace Inventory.Core.Entities;

/// <summary>
/// Inventory product. Maintains ONE consolidated stock balance regardless of which department
/// consumes it. Department-level consumption is tracked via StockIssueItem.Purpose.
/// Inherits Id, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy from BaseEntity.
/// </summary>
public class Product : BaseEntity
{
    /// <summary>Unique product code / SKU. Max 50 chars.</summary>
    public string ProductCode { get; set; } = string.Empty;

    public string ProductName { get; set; } = string.Empty;

    public int CategoryId { get; set; }

    public int UnitId { get; set; }

    /// <summary>
    /// Running stock balance. Precision: decimal(18,3).
    /// Updated by application logic during Purchase receipts and Stock Issues.
    /// </summary>
    public decimal CurrentStock { get; set; } = 0m;

    /// <summary>
    /// Alert threshold. Precision: decimal(18,3).
    /// Used to flag low-stock conditions.
    /// </summary>
    public decimal MinimumStock { get; set; } = 0m;

    /// <summary>
    /// Most recent purchase unit price. Precision: decimal(18,2).
    /// Updated when a purchase is recorded.
    /// </summary>
    public decimal LastPurchasePrice { get; set; } = 0m;

    public bool IsActive { get; set; } = true;

    // ── Navigation ─────────────────────────────────────────────────────────
    public Category Category { get; set; } = null!;
    public Unit Unit { get; set; } = null!;
    public ICollection<PurchaseItem> PurchaseItems { get; set; } = new List<PurchaseItem>();
    public ICollection<StoreRequestItem> StoreRequestItems { get; set; } = new List<StoreRequestItem>();
    public ICollection<StockIssueItem> StockIssueItems { get; set; } = new List<StockIssueItem>();
    public ICollection<StockTransaction> StockTransactions { get; set; } = new List<StockTransaction>();
}
