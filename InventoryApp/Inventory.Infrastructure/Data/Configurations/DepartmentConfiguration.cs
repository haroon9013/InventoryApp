using Inventory.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventory.Infrastructure.Data.Configurations;

public sealed class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.ToTable("Departments");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.DepartmentName)
               .IsRequired()
               .HasMaxLength(100);

        builder.Property(d => d.Description)
               .HasMaxLength(250);

        builder.HasIndex(d => d.DepartmentName)
               .IsUnique()
               .HasDatabaseName("IX_Departments_DepartmentName");
    }
}
