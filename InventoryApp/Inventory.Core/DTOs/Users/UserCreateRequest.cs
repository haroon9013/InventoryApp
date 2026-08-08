namespace Inventory.Core.DTOs.Users;

public sealed class UserCreateRequest
{
    public string FullName { get; init; } = string.Empty;
    public string UserName { get; init; } = string.Empty;

    /// <summary>Plain-text password supplied by Admin. Hashed in the service layer.</summary>
    public string Password { get; init; } = string.Empty;

    public int RoleId { get; init; }
    public bool IsActive { get; init; } = true;
}
