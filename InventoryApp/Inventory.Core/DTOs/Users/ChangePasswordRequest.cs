namespace Inventory.Core.DTOs.Users;

public sealed class ChangePasswordRequest
{
    public string CurrentPassword { get; init; } = string.Empty;
    public string NewPassword { get; init; } = string.Empty;
}
