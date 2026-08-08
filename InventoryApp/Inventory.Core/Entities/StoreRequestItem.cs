namespace Inventory.Core.Entities;

/// <summary>
/// Line item within a Store Request. No audit fields — the header (StoreRequest) carries the audit trail.
/// </summary>
public class StoreRequestItem
{
    public int Id { get; set; }

    public int StoreRequestId { get; set; }

    public int ProductId { get; set; }

    /// <summary>Requested quantity. Precision: decimal(18,3).</summary>
    public decimal Quantity { get; set; }

    public string? Remarks { get; set; }

    // ── Navigation ─────────────────────────────────────────────────────────
    public StoreRequest StoreRequest { get; set; } = null!;
    public Product Product { get; set; } = null!;
}
