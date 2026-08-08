using Inventory.Core.DTOs.Users;
using Inventory.Shared.Common;

namespace Inventory.Core.Interfaces.Services;

/// <summary>
/// User management operations available to Admin and authenticated users.
/// </summary>
public interface IUserService
{
    Task<Result<IReadOnlyList<UserResponse>>> GetAllAsync(CancellationToken ct = default);

    Task<Result<UserResponse>> GetByIdAsync(int id, CancellationToken ct = default);

    Task<Result<UserResponse>> CreateAsync(UserCreateRequest request, int createdByUserId, CancellationToken ct = default);

    Task<Result<UserResponse>> UpdateAsync(int id, UserUpdateRequest request, int updatedByUserId, CancellationToken ct = default);

    /// <summary>Change own password — caller must supply current password for verification.</summary>
    Task<Result> ChangePasswordAsync(int userId, ChangePasswordRequest request, CancellationToken ct = default);

    /// <summary>Admin resets another user's password without verifying current password.</summary>
    Task<Result> AdminResetPasswordAsync(int targetUserId, AdminResetPasswordRequest request, CancellationToken ct = default);
}
