namespace Inventory.Core.Entities;

/// <summary>
/// Application role. Does not carry audit timestamps — roles are system-managed reference data.
/// </summary>
public class Role
{
    public int Id { get; set; }

    /// <summary>Human-readable role name. Max 50 chars, unique.</summary>
    public string Name { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    // ── Navigation ─────────────────────────────────────────────────────────
    public ICollection<User> Users { get; set; } = new List<User>();
}
