using Inventory.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventory.Infrastructure.Data.Configurations;

public sealed class StoreRequestConfiguration : IEntityTypeConfiguration<StoreRequest>
{
    public void Configure(EntityTypeBuilder<StoreRequest> builder)
    {
        builder.ToTable("StoreRequests");

        builder.HasKey(sr => sr.Id);

        builder.Property(sr => sr.RequestNo)
               .IsRequired()
               .HasMaxLength(30);

        builder.Property(sr => sr.Remarks)
               .HasMaxLength(500);

        builder.Property(sr => sr.RequestDate)
               .IsRequired();

        // Status stored as int (EF Core default for enums).
        builder.Property(sr => sr.Status)
               .IsRequired();

        builder.HasIndex(sr => sr.RequestNo)
               .IsUnique()
               .HasDatabaseName("IX_StoreRequests_RequestNo");

        builder.HasIndex(sr => new { sr.Status, sr.DepartmentId })
               .HasDatabaseName("IX_StoreRequests_Status_Department");

        // ── Relationships ──────────────────────────────────────────────────
        builder.HasOne(sr => sr.Department)
               .WithMany(d => d.StoreRequests)
               .HasForeignKey(sr => sr.DepartmentId)
               .OnDelete(DeleteBehavior.Restrict);

        // Three separate FK relationships to User: RequestedBy, ApprovedBy, RejectedBy.
        // No reverse collection on User to avoid complexity.
        builder.HasOne(sr => sr.RequestedByUser)
               .WithMany()
               .HasForeignKey(sr => sr.RequestedBy)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(sr => sr.ApprovedByUser)
               .WithMany()
               .HasForeignKey(sr => sr.ApprovedBy)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(sr => sr.RejectedByUser)
               .WithMany()
               .HasForeignKey(sr => sr.RejectedBy)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
