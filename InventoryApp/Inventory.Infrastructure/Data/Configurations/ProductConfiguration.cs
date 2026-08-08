using Inventory.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventory.Infrastructure.Data.Configurations;

public sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.ProductCode)
               .IsRequired()
               .HasMaxLength(50);

        builder.Property(p => p.ProductName)
               .IsRequired()
               .HasMaxLength(200);

        // Decimal precision for stock quantities and pricing
        builder.Property(p => p.CurrentStock)
               .HasPrecision(18, 3);

        builder.Property(p => p.MinimumStock)
               .HasPrecision(18, 3);

        builder.Property(p => p.LastPurchasePrice)
               .HasPrecision(18, 2);

        // Unique product code / SKU
        builder.HasIndex(p => p.ProductCode)
               .IsUnique()
               .HasDatabaseName("IX_Products_ProductCode");

        // ── Relationships ──────────────────────────────────────────────────
        // Restrict delete on Category — deactivate the category rather than delete it.
        builder.HasOne(p => p.Category)
               .WithMany(c => c.Products)
               .HasForeignKey(p => p.CategoryId)
               .OnDelete(DeleteBehavior.Restrict);

        // Restrict delete on Unit — deactivate the unit rather than delete it.
        builder.HasOne(p => p.Unit)
               .WithMany(u => u.Products)
               .HasForeignKey(p => p.UnitId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
