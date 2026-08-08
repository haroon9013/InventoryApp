namespace Inventory.Core.Entities;

/// <summary>
/// Stock Issue header — records physical dispatch of items from the store to a department.
/// StoreRequestId is nullable to allow ad-hoc issues without a preceding formal request.
/// Immutable after creation — no UpdatedAt.
/// </summary>
public class StockIssue
{
    public int Id { get; set; }

    /// <summary>Unique issue reference number. Max 30 chars.</summary>
    public string IssueNo { get; set; } = string.Empty;

    /// <summary>Nullable link to an originating StoreRequest.</summary>
    public int? StoreRequestId { get; set; }

    public int DepartmentId { get; set; }

    /// <summary>FK to User — the store-keeper who issued stock.</summary>
    public int IssuedBy { get; set; }

    /// <summary>FK to User — the department person who received stock. Nullable.</summary>
    public int? ReceivedBy { get; set; }

    public DateTime IssueDate { get; set; }

    public string? Remarks { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // ── Navigation ─────────────────────────────────────────────────────────
    public StoreRequest? StoreRequest { get; set; }
    public Department Department { get; set; } = null!;
    public User IssuedByUser { get; set; } = null!;
    public User? ReceivedByUser { get; set; }
    public ICollection<StockIssueItem> StockIssueItems { get; set; } = new List<StockIssueItem>();
}
