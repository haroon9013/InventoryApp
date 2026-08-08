using Inventory.Core.DTOs.Auth;
using Inventory.Core.Interfaces.Services;
using Inventory.Shared.Common;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<LoginResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var result = await _authService.LoginAsync(request, ct);

        if (!result.IsSuccess)
        {
            // Security requires distinct messages are not leaked (e.g. invalid username vs bad password),
            // which is handled inside AuthService. But if it's inactive or invalid credentials, we return 401.
            if (result.Error == "User account is inactive.")
            {
                return Unauthorized(ApiResponse.Fail(result.Error));
            }
            return Unauthorized(ApiResponse.Fail(result.Error ?? "Authentication failed."));
        }

        return Ok(ApiResponse<LoginResponse>.Ok(result.Value!));
    }
}
