namespace Inventory.Core.Entities;

/// <summary>
/// Application user. Inherits Id, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy from BaseEntity.
/// Authentication implementation is deferred to a later phase.
/// </summary>
public class User : BaseEntity
{
    public string FullName { get; set; } = string.Empty;

    /// <summary>Unique login handle. Max 100 chars.</summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>BCrypt hash. Never store plain-text passwords.</summary>
    public string PasswordHash { get; set; } = string.Empty;

    public int RoleId { get; set; }

    public bool IsActive { get; set; } = true;

    // ── Navigation ─────────────────────────────────────────────────────────
    public Role Role { get; set; } = null!;
}
