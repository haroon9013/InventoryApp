using Inventory.Core.DTOs.Categories;
using Inventory.Shared.Common;

namespace Inventory.Core.Interfaces.Services;

public interface ICategoryService
{
    Task<Result<IReadOnlyList<CategoryResponse>>> GetAllAsync(bool includeInactive = false, CancellationToken ct = default);
    Task<Result<CategoryResponse>> GetByIdAsync(int id, bool includeInactive = false, CancellationToken ct = default);
    Task<Result<CategoryResponse>> CreateAsync(CategoryCreateRequest request, int createdByUserId, CancellationToken ct = default);
    Task<Result<CategoryResponse>> UpdateAsync(int id, CategoryUpdateRequest request, int updatedByUserId, CancellationToken ct = default);
    Task<Result> SoftDeleteAsync(int id, int deletedByUserId, CancellationToken ct = default);
}
