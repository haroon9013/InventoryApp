namespace Inventory.Core.Entities;

/// <summary>
/// Line item within a Purchase. No audit fields — the header (Purchase) carries the audit trail.
/// Amount = Quantity × Price, kept as a stored value for reporting convenience.
/// </summary>
public class PurchaseItem
{
    public int Id { get; set; }

    public int PurchaseId { get; set; }

    public int ProductId { get; set; }

    /// <summary>Received quantity. Precision: decimal(18,3).</summary>
    public decimal Quantity { get; set; }

    /// <summary>Unit price at time of purchase. Precision: decimal(18,2).</summary>
    public decimal Price { get; set; }

    /// <summary>Quantity × Price. Precision: decimal(18,2).</summary>
    public decimal Amount { get; set; }

    // ── Navigation ─────────────────────────────────────────────────────────
    public Purchase Purchase { get; set; } = null!;
    public Product Product { get; set; } = null!;
}
