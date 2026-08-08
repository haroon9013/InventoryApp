using Inventory.Core.DTOs.Units;
using Inventory.Core.Interfaces.Services;
using Inventory.Shared.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Inventory.Api.Controllers;

[ApiController]
[Route("api/units")]
[Authorize]
public sealed class UnitsController : ControllerBase
{
    private readonly IUnitService _unitService;

    public UnitsController(IUnitService unitService)
    {
        _unitService = unitService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<UnitResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _unitService.GetAllAsync(ct);
        return Ok(ApiResponse<IReadOnlyList<UnitResponse>>.Ok(result.Value!));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<UnitResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById([FromRoute] int id, CancellationToken ct)
    {
        var result = await _unitService.GetByIdAsync(id, ct);
        if (!result.IsSuccess)
        {
            return NotFound(ApiResponse.Fail(result.Error ?? "Unit not found."));
        }
        return Ok(ApiResponse<UnitResponse>.Ok(result.Value!));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<UnitResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] UnitCreateRequest request, CancellationToken ct)
    {
        var currentUserId = GetCurrentUserId();
        var result = await _unitService.CreateAsync(request, currentUserId, ct);

        if (!result.IsSuccess)
        {
            if (result.Error == "An active unit with this name already exists." || 
                result.Error == "An active unit with this short name already exists.")
            {
                return Conflict(ApiResponse.Fail(result.Error));
            }
            return BadRequest(ApiResponse.Fail(result.Error ?? "Failed to create unit."));
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, ApiResponse<UnitResponse>.Ok(result.Value));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<UnitResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] UnitUpdateRequest request, CancellationToken ct)
    {
        var currentUserId = GetCurrentUserId();
        var result = await _unitService.UpdateAsync(id, request, currentUserId, ct);

        if (!result.IsSuccess)
        {
            if (result.Error == "Unit not found.")
            {
                return NotFound(ApiResponse.Fail(result.Error));
            }
            if (result.Error == "An active unit with this name already exists." || 
                result.Error == "An active unit with this short name already exists.")
            {
                return Conflict(ApiResponse.Fail(result.Error));
            }
            return BadRequest(ApiResponse.Fail(result.Error ?? "Failed to update unit."));
        }

        return Ok(ApiResponse<UnitResponse>.Ok(result.Value!));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete([FromRoute] int id, CancellationToken ct)
    {
        var currentUserId = GetCurrentUserId();
        var result = await _unitService.SoftDeleteAsync(id, currentUserId, ct);

        if (!result.IsSuccess)
        {
            return NotFound(ApiResponse.Fail(result.Error ?? "Unit not found."));
        }

        return Ok(ApiResponse.Ok("Unit successfully deactivated."));
    }

    private int GetCurrentUserId()
    {
        var idStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(idStr, out var id) ? id : 0;
    }
}
