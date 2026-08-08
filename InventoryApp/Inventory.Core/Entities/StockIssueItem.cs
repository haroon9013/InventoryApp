namespace Inventory.Core.Entities;

/// <summary>
/// Line item within a Stock Issue. No audit fields — the header (StockIssue) carries the audit trail.
/// Purpose records the intended consumption use (e.g. "Biryani", "Chinese") for reporting.
/// This is consumption metadata only — it does NOT create separate stock balances per purpose.
/// </summary>
public class StockIssueItem
{
    public int Id { get; set; }

    public int StockIssueId { get; set; }

    public int ProductId { get; set; }

    /// <summary>Quantity issued. Precision: decimal(18,3).</summary>
    public decimal Quantity { get; set; }

    /// <summary>
    /// Optional consumption purpose. Examples: "Biryani", "Chinese", "Meals".
    /// Used for consumption reporting — does NOT create separate inventory accounts.
    /// </summary>
    public string? Purpose { get; set; }

    public string? Remarks { get; set; }

    // ── Navigation ─────────────────────────────────────────────────────────
    public StockIssue StockIssue { get; set; } = null!;
    public Product Product { get; set; } = null!;
}
