using Inventory.Core.Entities;

namespace Inventory.Core.Interfaces.Services;

/// <summary>
/// Generates and validates JWT tokens.
/// Lives in Core so services can depend on it without referencing Infrastructure.
/// </summary>
public interface IJwtService
{
    /// <summary>Generates a signed JWT for the given user.</summary>
    string GenerateToken(User user, string roleName);

    /// <summary>Returns the UTC expiry time for a freshly generated token.</summary>
    DateTime GetExpiryTime();
}
