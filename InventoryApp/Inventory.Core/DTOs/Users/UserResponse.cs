namespace Inventory.Core.DTOs.Users;

/// <summary>
/// Safe user representation — PasswordHash is never included.
/// </summary>
public sealed class UserResponse
{
    public int Id { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string UserName { get; init; } = string.Empty;
    public int RoleId { get; init; }
    public string RoleName { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}
