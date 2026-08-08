namespace Inventory.Core.DTOs.Users;

public sealed class AdminResetPasswordRequest
{
    /// <summary>New plain-text password supplied by Admin. Hashed in the service layer.</summary>
    public string NewPassword { get; init; } = string.Empty;
}
