using Inventory.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventory.Infrastructure.Data.Configurations;

public sealed class UnitConfiguration : IEntityTypeConfiguration<Unit>
{
    public void Configure(EntityTypeBuilder<Unit> builder)
    {
        builder.ToTable("Units");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.UnitName)
               .IsRequired()
               .HasMaxLength(100);

        builder.Property(u => u.ShortName)
               .IsRequired()
               .HasMaxLength(20);

        builder.HasIndex(u => u.UnitName)
               .IsUnique()
               .HasDatabaseName("IX_Units_UnitName");

        builder.HasIndex(u => u.ShortName)
               .IsUnique()
               .HasDatabaseName("IX_Units_ShortName");
    }
}
