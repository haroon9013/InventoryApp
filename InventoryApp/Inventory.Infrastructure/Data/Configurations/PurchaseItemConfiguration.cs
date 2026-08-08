using Inventory.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventory.Infrastructure.Data.Configurations;

public sealed class PurchaseItemConfiguration : IEntityTypeConfiguration<PurchaseItem>
{
    public void Configure(EntityTypeBuilder<PurchaseItem> builder)
    {
        builder.ToTable("PurchaseItems");

        builder.HasKey(pi => pi.Id);

        builder.Property(pi => pi.Quantity)
               .HasPrecision(18, 3);

        builder.Property(pi => pi.Price)
               .HasPrecision(18, 2);

        builder.Property(pi => pi.Amount)
               .HasPrecision(18, 2);

        // ── Relationships ──────────────────────────────────────────────────
        // Cascade: deleting a purchase header removes its line items.
        builder.HasOne(pi => pi.Purchase)
               .WithMany(p => p.PurchaseItems)
               .HasForeignKey(pi => pi.PurchaseId)
               .OnDelete(DeleteBehavior.Cascade);

        // Restrict: product cannot be deleted while purchase history references it.
        builder.HasOne(pi => pi.Product)
               .WithMany(p => p.PurchaseItems)
               .HasForeignKey(pi => pi.ProductId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
