using Inventory.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventory.Infrastructure.Data.Configurations;

public sealed class PurchaseConfiguration : IEntityTypeConfiguration<Purchase>
{
    public void Configure(EntityTypeBuilder<Purchase> builder)
    {
        builder.ToTable("Purchases");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.PurchaseNo)
               .IsRequired()
               .HasMaxLength(30);

        builder.Property(p => p.TotalAmount)
               .HasPrecision(18, 2);

        builder.Property(p => p.PurchaseDate)
               .IsRequired();

        builder.Property(p => p.CreatedAt)
               .IsRequired();

        builder.HasIndex(p => p.PurchaseNo)
               .IsUnique()
               .HasDatabaseName("IX_Purchases_PurchaseNo");

        // ── Relationships ──────────────────────────────────────────────────
        // Restrict: supplier cannot be deleted while purchases reference them.
        builder.HasOne(p => p.Supplier)
               .WithMany(s => s.Purchases)
               .HasForeignKey(p => p.SupplierId)
               .OnDelete(DeleteBehavior.Restrict);

        // Restrict: user cannot be deleted while they own purchase records.
        builder.HasOne(p => p.CreatedByUser)
               .WithMany()
               .HasForeignKey(p => p.CreatedBy)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
