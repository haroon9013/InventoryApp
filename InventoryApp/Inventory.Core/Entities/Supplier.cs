namespace Inventory.Core.Entities;

/// <summary>
/// Product supplier / vendor.
/// Inherits Id, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy from BaseEntity.
/// Prefer deactivation over physical deletion when purchases reference this supplier.
/// </summary>
public class Supplier : BaseEntity
{
    public string SupplierName { get; set; } = string.Empty;

    public string? ContactPerson { get; set; }

    public string? Mobile { get; set; }

    public string? Address { get; set; }

    /// <summary>
    /// GST registration number. Nullable — not all suppliers may be GST-registered.
    /// </summary>
    public string? GSTNumber { get; set; }

    public bool IsActive { get; set; } = true;

    // ── Navigation ─────────────────────────────────────────────────────────
    public ICollection<Purchase> Purchases { get; set; } = new List<Purchase>();
}
