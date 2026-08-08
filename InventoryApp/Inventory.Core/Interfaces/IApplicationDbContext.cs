using Inventory.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Core.Interfaces;

/// <summary>
/// Abstraction over the EF Core DbContext exposed to the Core layer.
/// Keeps Core free of a direct EF dependency while allowing query composition.
/// </summary>
public interface IApplicationDbContext
{
    DbSet<Role> Roles { get; }
    DbSet<User> Users { get; }
    DbSet<Category> Categories { get; }
    DbSet<Unit> Units { get; }
    DbSet<Department> Departments { get; }
    DbSet<Supplier> Suppliers { get; }
    DbSet<Product> Products { get; }
    DbSet<Purchase> Purchases { get; }
    DbSet<PurchaseItem> PurchaseItems { get; }
    DbSet<StoreRequest> StoreRequests { get; }
    DbSet<StoreRequestItem> StoreRequestItems { get; }
    DbSet<StockIssue> StockIssues { get; }
    DbSet<StockIssueItem> StockIssueItems { get; }
    DbSet<StockTransaction> StockTransactions { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
