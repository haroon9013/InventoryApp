using Inventory.Core.DTOs.Users;
using Inventory.Core.Interfaces.Services;
using Inventory.Shared.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Inventory.Api.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public sealed class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<UserResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _userService.GetAllAsync(ct);
        return Ok(ApiResponse<IReadOnlyList<UserResponse>>.Ok(result.Value!));
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<UserResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById([FromRoute] int id, CancellationToken ct)
    {
        var result = await _userService.GetByIdAsync(id, ct);
        if (!result.IsSuccess)
        {
            return NotFound(ApiResponse.Fail(result.Error ?? "User not found."));
        }
        return Ok(ApiResponse<UserResponse>.Ok(result.Value!));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<UserResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] UserCreateRequest request, CancellationToken ct)
    {
        var currentUserId = GetCurrentUserId();
        var result = await _userService.CreateAsync(request, currentUserId, ct);

        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse.Fail(result.Error ?? "Failed to create user."));
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, ApiResponse<UserResponse>.Ok(result.Value));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<UserResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] UserUpdateRequest request, CancellationToken ct)
    {
        var currentUserId = GetCurrentUserId();
        var result = await _userService.UpdateAsync(id, request, currentUserId, ct);

        if (!result.IsSuccess)
        {
            if (result.Error == "User not found.")
            {
                return NotFound(ApiResponse.Fail(result.Error));
            }
            return BadRequest(ApiResponse.Fail(result.Error ?? "Failed to update user."));
        }

        return Ok(ApiResponse<UserResponse>.Ok(result.Value!));
    }

    [HttpPost("change-password")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request, CancellationToken ct)
    {
        var currentUserId = GetCurrentUserId();
        var result = await _userService.ChangePasswordAsync(currentUserId, request, ct);

        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse.Fail(result.Error ?? "Failed to change password."));
        }

        return Ok(ApiResponse.Ok("Password changed successfully."));
    }

    [HttpPost("{id:int}/reset-password")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ResetPassword([FromRoute] int id, [FromBody] AdminResetPasswordRequest request, CancellationToken ct)
    {
        var result = await _userService.AdminResetPasswordAsync(id, request, ct);

        if (!result.IsSuccess)
        {
            if (result.Error == "User not found.")
            {
                return NotFound(ApiResponse.Fail(result.Error));
            }
            return BadRequest(ApiResponse.Fail(result.Error ?? "Failed to reset password."));
        }

        return Ok(ApiResponse.Ok("User password has been successfully reset."));
    }

    private int GetCurrentUserId()
    {
        var idStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(idStr, out var id) ? id : 0;
    }
}
