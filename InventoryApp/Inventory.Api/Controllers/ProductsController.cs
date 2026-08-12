using Inventory.Core.DTOs.Products;
using Inventory.Core.Interfaces.Services;
using Inventory.Shared.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Inventory.Api.Controllers;

[ApiController]
[Route("api/products")]
[Authorize]
public sealed class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<ProductResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll([FromQuery] bool includeInactive = false, CancellationToken ct = default)
    {
        var isAdmin = User.IsInRole("Admin");
        var actualIncludeInactive = isAdmin && includeInactive;
        var result = await _productService.GetAllAsync(actualIncludeInactive, ct);
        return Ok(ApiResponse<IReadOnlyList<ProductResponse>>.Ok(result.Value!));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<ProductResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById([FromRoute] int id, CancellationToken ct)
    {
        var isAdmin = User.IsInRole("Admin");
        var result = await _productService.GetByIdAsync(id, isAdmin, ct);
        if (!result.IsSuccess)
        {
            return NotFound(ApiResponse.Fail(result.Error ?? "Product not found."));
        }
        return Ok(ApiResponse<ProductResponse>.Ok(result.Value!));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<ProductResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] ProductCreateRequest request, CancellationToken ct)
    {
        var currentUserId = GetCurrentUserId();
        var result = await _productService.CreateAsync(request, currentUserId, ct);

        if (!result.IsSuccess)
        {
            if (result.Error == "A product with this product code already exists.")
            {
                return Conflict(ApiResponse.Fail(result.Error));
            }
            return BadRequest(ApiResponse.Fail(result.Error ?? "Failed to create product."));
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, ApiResponse<ProductResponse>.Ok(result.Value));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<ProductResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] ProductUpdateRequest request, CancellationToken ct)
    {
        var currentUserId = GetCurrentUserId();
        var result = await _productService.UpdateAsync(id, request, currentUserId, ct);

        if (!result.IsSuccess)
        {
            if (result.Error == "Product not found.")
            {
                return NotFound(ApiResponse.Fail(result.Error));
            }
            if (result.Error == "A product with this product code already exists.")
            {
                return Conflict(ApiResponse.Fail(result.Error));
            }
            return BadRequest(ApiResponse.Fail(result.Error ?? "Failed to update product."));
        }

        return Ok(ApiResponse<ProductResponse>.Ok(result.Value!));
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
        var result = await _productService.SoftDeleteAsync(id, currentUserId, ct);

        if (!result.IsSuccess)
        {
            return NotFound(ApiResponse.Fail(result.Error ?? "Product not found."));
        }

        return Ok(ApiResponse.Ok("Product successfully deactivated."));
    }

    private int GetCurrentUserId()
    {
        var idStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(idStr, out var id) ? id : 0;
    }
}
