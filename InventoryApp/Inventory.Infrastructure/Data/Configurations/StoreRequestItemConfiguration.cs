using Inventory.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventory.Infrastructure.Data.Configurations;

public sealed class StoreRequestItemConfiguration : IEntityTypeConfiguration<StoreRequestItem>
{
    public void Configure(EntityTypeBuilder<StoreRequestItem> builder)
    {
        builder.ToTable("StoreRequestItems");

        builder.HasKey(sri => sri.Id);

        builder.Property(sri => sri.Quantity)
               .HasPrecision(18, 3);

        builder.Property(sri => sri.Remarks)
               .HasMaxLength(500);

        // ── Relationships ──────────────────────────────────────────────────
        // Cascade: deleting a request header removes its line items.
        builder.HasOne(sri => sri.StoreRequest)
               .WithMany(sr => sr.StoreRequestItems)
               .HasForeignKey(sri => sri.StoreRequestId)
               .OnDelete(DeleteBehavior.Cascade);

        // Restrict: product cannot be deleted while request history references it.
        builder.HasOne(sri => sri.Product)
               .WithMany(p => p.StoreRequestItems)
               .HasForeignKey(sri => sri.ProductId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
