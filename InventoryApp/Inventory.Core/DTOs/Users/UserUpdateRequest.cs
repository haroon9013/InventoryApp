namespace Inventory.Core.DTOs.Users;

public sealed class UserUpdateRequest
{
    public string FullName { get; init; } = string.Empty;
    public string UserName { get; init; } = string.Empty;
    public int RoleId { get; init; }
    public bool IsActive { get; init; }
}
