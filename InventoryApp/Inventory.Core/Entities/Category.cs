namespace Inventory.Core.Entities;

/// <summary>
/// Product category (e.g. Spices, Dairy, Beverages).
/// Inherits Id, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy from BaseEntity.
/// Prefer deactivation over physical deletion when products exist in the category.
/// </summary>
public class Category : BaseEntity
{
    /// <summary>Unique among active categories (filtered unique index).</summary>
    public string CategoryName { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    // ── Navigation ─────────────────────────────────────────────────────────
    public ICollection<Product> Products { get; set; } = new List<Product>();
}
