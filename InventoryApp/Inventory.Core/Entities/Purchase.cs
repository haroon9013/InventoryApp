namespace Inventory.Core.Entities;

/// <summary>
/// Purchase header (GRN). Immutable after creation — no UpdatedAt.
/// CreatedBy is a required (non-nullable) FK to the user who raised the purchase.
/// </summary>
public class Purchase
{
    public int Id { get; set; }

    /// <summary>Unique purchase/GRN number. Max 30 chars.</summary>
    public string PurchaseNo { get; set; } = string.Empty;

    public int SupplierId { get; set; }

    public DateTime PurchaseDate { get; set; }

    /// <summary>Total invoice value. Precision: decimal(18,2).</summary>
    public decimal TotalAmount { get; set; }

    /// <summary>FK to User. Required — every purchase must have an owner.</summary>
    public int CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // ── Navigation ─────────────────────────────────────────────────────────
    public Supplier Supplier { get; set; } = null!;
    public User CreatedByUser { get; set; } = null!;
    public ICollection<PurchaseItem> PurchaseItems { get; set; } = new List<PurchaseItem>();
}
