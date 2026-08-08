using Inventory.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventory.Infrastructure.Data.Configurations;

public sealed class StockIssueConfiguration : IEntityTypeConfiguration<StockIssue>
{
    public void Configure(EntityTypeBuilder<StockIssue> builder)
    {
        builder.ToTable("StockIssues");

        builder.HasKey(si => si.Id);

        builder.Property(si => si.IssueNo)
               .IsRequired()
               .HasMaxLength(30);

        builder.Property(si => si.Remarks)
               .HasMaxLength(500);

        builder.Property(si => si.IssueDate)
               .IsRequired();

        builder.Property(si => si.CreatedAt)
               .IsRequired();

        builder.HasIndex(si => si.IssueNo)
               .IsUnique()
               .HasDatabaseName("IX_StockIssues_IssueNo");

        builder.HasIndex(si => new { si.DepartmentId, si.IssueDate })
               .HasDatabaseName("IX_StockIssues_Department_Date");

        // ── Relationships ──────────────────────────────────────────────────
        // StoreRequestId is nullable — issues can be ad-hoc.
        // Restrict: cannot delete a StoreRequest that has issues linked to it.
        builder.HasOne(si => si.StoreRequest)
               .WithMany(sr => sr.StockIssues)
               .HasForeignKey(si => si.StoreRequestId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(si => si.Department)
               .WithMany(d => d.StockIssues)
               .HasForeignKey(si => si.DepartmentId)
               .OnDelete(DeleteBehavior.Restrict);

        // Two FK relationships to User: IssuedBy, ReceivedBy.
        builder.HasOne(si => si.IssuedByUser)
               .WithMany()
               .HasForeignKey(si => si.IssuedBy)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(si => si.ReceivedByUser)
               .WithMany()
               .HasForeignKey(si => si.ReceivedBy)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
