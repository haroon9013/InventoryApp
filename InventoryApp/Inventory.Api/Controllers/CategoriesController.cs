using Inventory.Core.DTOs.Categories;
using Inventory.Core.Interfaces.Services;
using Inventory.Shared.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Inventory.Api.Controllers;

[ApiController]
[Route("api/categories")]
[Authorize]
public sealed class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<CategoryResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _categoryService.GetAllAsync(ct);
        return Ok(ApiResponse<IReadOnlyList<CategoryResponse>>.Ok(result.Value!));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<CategoryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById([FromRoute] int id, CancellationToken ct)
    {
        var result = await _categoryService.GetByIdAsync(id, ct);
        if (!result.IsSuccess)
        {
            return NotFound(ApiResponse.Fail(result.Error ?? "Category not found."));
        }
        return Ok(ApiResponse<CategoryResponse>.Ok(result.Value!));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<CategoryResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CategoryCreateRequest request, CancellationToken ct)
    {
        var currentUserId = GetCurrentUserId();
        var result = await _categoryService.CreateAsync(request, currentUserId, ct);

        if (!result.IsSuccess)
        {
            if (result.Error == "An active category with this name already exists.")
            {
                return Conflict(ApiResponse.Fail(result.Error));
            }
            return BadRequest(ApiResponse.Fail(result.Error ?? "Failed to create category."));
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, ApiResponse<CategoryResponse>.Ok(result.Value));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<CategoryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] CategoryUpdateRequest request, CancellationToken ct)
    {
        var currentUserId = GetCurrentUserId();
        var result = await _categoryService.UpdateAsync(id, request, currentUserId, ct);

        if (!result.IsSuccess)
        {
            if (result.Error == "Category not found.")
            {
                return NotFound(ApiResponse.Fail(result.Error));
            }
            if (result.Error == "An active category with this name already exists.")
            {
                return Conflict(ApiResponse.Fail(result.Error));
            }
            return BadRequest(ApiResponse.Fail(result.Error ?? "Failed to update category."));
        }

        return Ok(ApiResponse<CategoryResponse>.Ok(result.Value!));
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
        var result = await _categoryService.SoftDeleteAsync(id, currentUserId, ct);

        if (!result.IsSuccess)
        {
            return NotFound(ApiResponse.Fail(result.Error ?? "Category not found."));
        }

        return Ok(ApiResponse.Ok("Category successfully deactivated."));
    }

    private int GetCurrentUserId()
    {
        var idStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(idStr, out var id) ? id : 0;
    }
}
