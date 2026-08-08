using Inventory.Core.Enums;

namespace Inventory.Core.Entities;

/// <summary>
/// Department request for stock items from the central store.
/// Lifecycle status is tracked via the Status enum.
/// Inherits Id, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy from BaseEntity.
/// </summary>
public class StoreRequest : BaseEntity
{
    /// <summary>Unique request reference number. Max 30 chars.</summary>
    public string RequestNo { get; set; } = string.Empty;

    public int DepartmentId { get; set; }

    /// <summary>FK to User — the department person who raised the request.</summary>
    public int RequestedBy { get; set; }

    public DateTime RequestDate { get; set; }

    public StoreRequestStatus Status { get; set; } = StoreRequestStatus.Pending;

    /// <summary>Nullable FK to User — populated when status transitions to Approved.</summary>
    public int? ApprovedBy { get; set; }

    public DateTime? ApprovedDate { get; set; }

    /// <summary>Nullable FK to User — populated when status transitions to Rejected.</summary>
    public int? RejectedBy { get; set; }

    public DateTime? RejectedDate { get; set; }

    public string? Remarks { get; set; }

    // ── Navigation ─────────────────────────────────────────────────────────
    public Department Department { get; set; } = null!;
    public User RequestedByUser { get; set; } = null!;
    public User? ApprovedByUser { get; set; }
    public User? RejectedByUser { get; set; }
    public ICollection<StoreRequestItem> StoreRequestItems { get; set; } = new List<StoreRequestItem>();
    public ICollection<StockIssue> StockIssues { get; set; } = new List<StockIssue>();
}
