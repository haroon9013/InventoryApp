using Inventory.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventory.Infrastructure.Data.Configurations;

public sealed class StockIssueItemConfiguration : IEntityTypeConfiguration<StockIssueItem>
{
    public void Configure(EntityTypeBuilder<StockIssueItem> builder)
    {
        builder.ToTable("StockIssueItems");

        builder.HasKey(sii => sii.Id);

        builder.Property(sii => sii.Quantity)
               .HasPrecision(18, 3);

        builder.Property(sii => sii.Purpose)
               .HasMaxLength(200);

        builder.Property(sii => sii.Remarks)
               .HasMaxLength(500);

        // ── Relationships ──────────────────────────────────────────────────
        // Cascade: deleting an issue header removes its line items.
        builder.HasOne(sii => sii.StockIssue)
               .WithMany(si => si.StockIssueItems)
               .HasForeignKey(sii => sii.StockIssueId)
               .OnDelete(DeleteBehavior.Cascade);

        // Restrict: product cannot be deleted while issue history references it.
        builder.HasOne(sii => sii.Product)
               .WithMany(p => p.StockIssueItems)
               .HasForeignKey(sii => sii.ProductId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
