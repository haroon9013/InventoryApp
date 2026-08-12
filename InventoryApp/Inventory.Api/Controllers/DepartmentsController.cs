using Inventory.Core.DTOs.Departments;
using Inventory.Core.Interfaces.Services;
using Inventory.Shared.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Inventory.Api.Controllers;

[ApiController]
[Route("api/departments")]
[Authorize]
public sealed class DepartmentsController : ControllerBase
{
    private readonly IDepartmentService _departmentService;

    public DepartmentsController(IDepartmentService departmentService)
    {
        _departmentService = departmentService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<DepartmentResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll([FromQuery] bool includeInactive = false, CancellationToken ct = default)
    {
        var isAdmin = User.IsInRole("Admin");
        var actualIncludeInactive = isAdmin && includeInactive;
        var result = await _departmentService.GetAllAsync(actualIncludeInactive, ct);
        return Ok(ApiResponse<IReadOnlyList<DepartmentResponse>>.Ok(result.Value!));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<DepartmentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById([FromRoute] int id, CancellationToken ct)
    {
        var isAdmin = User.IsInRole("Admin");
        var result = await _departmentService.GetByIdAsync(id, isAdmin, ct);
        if (!result.IsSuccess)
        {
            return NotFound(ApiResponse.Fail(result.Error ?? "Department not found."));
        }
        return Ok(ApiResponse<DepartmentResponse>.Ok(result.Value!));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<DepartmentResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] DepartmentCreateRequest request, CancellationToken ct)
    {
        var currentUserId = GetCurrentUserId();
        var result = await _departmentService.CreateAsync(request, currentUserId, ct);

        if (!result.IsSuccess)
        {
            if (result.Error == "An active department with this name already exists.")
            {
                return Conflict(ApiResponse.Fail(result.Error));
            }
            return BadRequest(ApiResponse.Fail(result.Error ?? "Failed to create department."));
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, ApiResponse<DepartmentResponse>.Ok(result.Value));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<DepartmentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] DepartmentUpdateRequest request, CancellationToken ct)
    {
        var currentUserId = GetCurrentUserId();
        var result = await _departmentService.UpdateAsync(id, request, currentUserId, ct);

        if (!result.IsSuccess)
        {
            if (result.Error == "Department not found.")
            {
                return NotFound(ApiResponse.Fail(result.Error));
            }
            if (result.Error == "An active department with this name already exists.")
            {
                return Conflict(ApiResponse.Fail(result.Error));
            }
            return BadRequest(ApiResponse.Fail(result.Error ?? "Failed to update department."));
        }

        return Ok(ApiResponse<DepartmentResponse>.Ok(result.Value!));
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
        var result = await _departmentService.SoftDeleteAsync(id, currentUserId, ct);

        if (!result.IsSuccess)
        {
            return NotFound(ApiResponse.Fail(result.Error ?? "Department not found."));
        }

        return Ok(ApiResponse.Ok("Department successfully deactivated."));
    }

    private int GetCurrentUserId()
    {
        var idStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(idStr, out var id) ? id : 0;
    }
}
