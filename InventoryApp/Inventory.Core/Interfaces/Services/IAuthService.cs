using Inventory.Core.DTOs.Auth;
using Inventory.Shared.Common;

namespace Inventory.Core.Interfaces.Services;

/// <summary>
/// Handles credential validation and token issuance.
/// </summary>
public interface IAuthService
{
    /// <summary>Validates credentials and returns a JWT on success.</summary>
    Task<Result<LoginResponse>> LoginAsync(LoginRequest request, CancellationToken ct = default);
}
