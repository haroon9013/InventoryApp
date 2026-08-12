using Inventory.Core.DTOs.Products;
using Inventory.Shared.Common;

namespace Inventory.Core.Interfaces.Services;

public interface IProductService
{
    Task<Result<IReadOnlyList<ProductResponse>>> GetAllAsync(bool includeInactive = false, CancellationToken ct = default);
    Task<Result<ProductResponse>> GetByIdAsync(int id, bool includeInactive = false, CancellationToken ct = default);
    Task<Result<ProductResponse>> CreateAsync(ProductCreateRequest request, int createdByUserId, CancellationToken ct = default);
    Task<Result<ProductResponse>> UpdateAsync(int id, ProductUpdateRequest request, int updatedByUserId, CancellationToken ct = default);
    Task<Result> SoftDeleteAsync(int id, int deletedByUserId, CancellationToken ct = default);
}
