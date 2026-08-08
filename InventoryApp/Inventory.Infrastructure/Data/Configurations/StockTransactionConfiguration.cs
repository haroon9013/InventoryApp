using Inventory.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventory.Infrastructure.Data.Configurations;

public sealed class StockTransactionConfiguration : IEntityTypeConfiguration<StockTransaction>
{
    public void Configure(EntityTypeBuilder<StockTransaction> builder)
    {
        builder.ToTable("StockTransactions");

        builder.HasKey(st => st.Id);

        // Enums stored as int (EF Core default).
        builder.Property(st => st.TransactionType)
               .IsRequired();

        builder.Property(st => st.ReferenceType);  // Nullable

        builder.Property(st => st.Quantity)
               .HasPrecision(18, 3);

        builder.Property(st => st.Purpose)
               .HasMaxLength(200);

        builder.Property(st => st.Remarks)
               .HasMaxLength(500);

        builder.Property(st => st.TransactionDate)
               .IsRequired();

        builder.Property(st => st.CreatedAt)
               .IsRequired();

        // ── Indexes (time-series query pattern) ───────────────────────────
        builder.HasIndex(st => new { st.ProductId, st.TransactionDate })
               .HasDatabaseName("IX_StockTransactions_Product_Date");

        builder.HasIndex(st => st.TransactionDate)
               .HasDatabaseName("IX_StockTransactions_Date");

        // ── Relationships ─────────────────────────────────────────────────
        // ALL relationships use Restrict — this is an immutable audit trail.
        // Nothing should cascade-delete transaction history.

        builder.HasOne(st => st.Product)
               .WithMany(p => p.StockTransactions)
               .HasForeignKey(st => st.ProductId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(st => st.Department)
               .WithMany(d => d.StockTransactions)
               .HasForeignKey(st => st.DepartmentId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(st => st.CreatedByUser)
               .WithMany()
               .HasForeignKey(st => st.CreatedBy)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
