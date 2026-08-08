using Inventory.Core.DTOs.Units;
using Inventory.Shared.Common;

namespace Inventory.Core.Interfaces.Services;

public interface IUnitService
{
    Task<Result<IReadOnlyList<UnitResponse>>> GetAllAsync(CancellationToken ct = default);
    Task<Result<UnitResponse>> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Result<UnitResponse>> CreateAsync(UnitCreateRequest request, int createdByUserId, CancellationToken ct = default);
    Task<Result<UnitResponse>> UpdateAsync(int id, UnitUpdateRequest request, int updatedByUserId, CancellationToken ct = default);
    Task<Result> SoftDeleteAsync(int id, int deletedByUserId, CancellationToken ct = default);
}
