namespace Inventory.Core.Entities;

/// <summary>
/// Hotel operational department (e.g. Kitchen, Store, Bakery, Bar).
/// Inherits Id, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy from BaseEntity.
/// </summary>
public class Department : BaseEntity
{
    /// <summary>Unique department name. Max 100 chars.</summary>
    public string DepartmentName { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    // ── Navigation ─────────────────────────────────────────────────────────
    public ICollection<StoreRequest> StoreRequests { get; set; } = new List<StoreRequest>();
    public ICollection<StockIssue> StockIssues { get; set; } = new List<StockIssue>();
    public ICollection<StockTransaction> StockTransactions { get; set; } = new List<StockTransaction>();
}
