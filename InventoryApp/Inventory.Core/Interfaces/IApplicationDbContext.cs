using Microsoft.EntityFrameworkCore;

namespace Inventory.Core.Interfaces;

/// <summary>
/// Abstraction over the EF Core DbContext exposed to the Core layer.
/// Keeps Core free of a direct EF dependency while allowing query composition.
/// </summary>
public interface IApplicationDbContext
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
