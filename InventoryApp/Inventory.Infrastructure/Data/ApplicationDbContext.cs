using Inventory.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Inventory.Infrastructure.Data;

/// <summary>
/// EF Core DbContext for InventoryApp.
/// Entities are registered via IEntityTypeConfiguration classes in Data/Configurations/.
/// </summary>
public sealed class ApplicationDbContext : DbContext, Core.Interfaces.IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    // ── Master / Reference Data ───────────────────────────────────────────
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Unit> Units => Set<Unit>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<Product> Products => Set<Product>();

    // ── Purchasing ────────────────────────────────────────────────────────
    public DbSet<Purchase> Purchases => Set<Purchase>();
    public DbSet<PurchaseItem> PurchaseItems => Set<PurchaseItem>();

    // ── Store Requests ────────────────────────────────────────────────────
    public DbSet<StoreRequest> StoreRequests => Set<StoreRequest>();
    public DbSet<StoreRequestItem> StoreRequestItems => Set<StoreRequestItem>();

    // ── Stock Issues ──────────────────────────────────────────────────────
    public DbSet<StockIssue> StockIssues => Set<StockIssue>();
    public DbSet<StockIssueItem> StockIssueItems => Set<StockIssueItem>();

    // ── Audit Trail ───────────────────────────────────────────────────────
    public DbSet<StockTransaction> StockTransactions => Set<StockTransaction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Applies all IEntityTypeConfiguration classes found in this assembly.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }

    /// <summary>
    /// Automatically stamps UpdatedAt on every modified BaseEntity before saving.
    /// Non-BaseEntity entities (Purchase, StockIssue, StockTransaction) carry
    /// their own CreatedAt with a C# default of DateTime.UtcNow.
    /// </summary>
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        StampAuditFields();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        StampAuditFields();
        return base.SaveChanges();
    }

    private void StampAuditFields()
    {
        foreach (EntityEntry<BaseEntity> entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = DateTime.UtcNow;
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }
        }
    }
}
