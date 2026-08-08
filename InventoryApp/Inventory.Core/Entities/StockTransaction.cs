using Inventory.Core.Enums;

namespace Inventory.Core.Entities;

/// <summary>
/// Permanent, immutable audit trail of all stock movements.
/// Records are never physically deleted. Cascading deletes are blocked on all FKs.
/// Product.CurrentStock provides fast access to current balance;
/// StockTransaction is the source of truth for movement history.
/// </summary>
public class StockTransaction
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    /// <summary>Stored as int (0=StockIn, 1=StockOut, 2=Adjustment).</summary>
    public TransactionType TransactionType { get; set; }

    /// <summary>Always positive. Direction is determined by TransactionType. Precision: decimal(18,3).</summary>
    public decimal Quantity { get; set; }

    /// <summary>Type of the source document. Nullable for manual adjustments.</summary>
    public ReferenceType? ReferenceType { get; set; }

    /// <summary>PK of the source document (Purchase.Id, StoreRequest.Id, etc.).</summary>
    public int? ReferenceId { get; set; }

    /// <summary>Nullable FK to Department — applicable for StockOut movements.</summary>
    public int? DepartmentId { get; set; }

    /// <summary>
    /// Optional consumption purpose (mirrors StockIssueItem.Purpose for StockOut entries).
    /// </summary>
    public string? Purpose { get; set; }

    public string? Remarks { get; set; }

    public DateTime TransactionDate { get; set; }

    /// <summary>FK to User. Required — every transaction must have an owner.</summary>
    public int CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // ── Navigation ─────────────────────────────────────────────────────────
    public Product Product { get; set; } = null!;
    public Department? Department { get; set; }
    public User CreatedByUser { get; set; } = null!;
}
