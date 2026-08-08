namespace Inventory.Core.DTOs.Auth;

public sealed class LoginResponse
{
    public string AccessToken { get; init; } = string.Empty;
    public string TokenType { get; init; } = "Bearer";
    public DateTime ExpiresAt { get; init; }
    public UserInfo User { get; init; } = null!;
}

public sealed class UserInfo
{
    public int Id { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string UserName { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
}
