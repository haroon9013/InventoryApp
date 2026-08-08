namespace Inventory.Core.Entities;

/// <summary>
/// Measurement unit (e.g. Kg, Gram, Litre, ml, Piece, Packet, Box).
/// Inherits Id, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy from BaseEntity.
/// Prefer deactivation over physical deletion when products reference this unit.
/// </summary>
public class Unit : BaseEntity
{
    /// <summary>Full unit name. Max 100 chars, unique.</summary>
    public string UnitName { get; set; } = string.Empty;

    /// <summary>Abbreviation used on documents. Max 20 chars, unique.</summary>
    public string ShortName { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    // ── Navigation ─────────────────────────────────────────────────────────
    public ICollection<Product> Products { get; set; } = new List<Product>();
}
