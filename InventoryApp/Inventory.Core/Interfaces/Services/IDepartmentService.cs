using Inventory.Core.DTOs.Departments;
using Inventory.Shared.Common;

namespace Inventory.Core.Interfaces.Services;

public interface IDepartmentService
{
    Task<Result<IReadOnlyList<DepartmentResponse>>> GetAllAsync(bool includeInactive = false, CancellationToken ct = default);
    Task<Result<DepartmentResponse>> GetByIdAsync(int id, bool includeInactive = false, CancellationToken ct = default);
    Task<Result<DepartmentResponse>> CreateAsync(DepartmentCreateRequest request, int createdByUserId, CancellationToken ct = default);
    Task<Result<DepartmentResponse>> UpdateAsync(int id, DepartmentUpdateRequest request, int updatedByUserId, CancellationToken ct = default);
    Task<Result> SoftDeleteAsync(int id, int deletedByUserId, CancellationToken ct = default);
}
