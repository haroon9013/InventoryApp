using Inventory.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventory.Infrastructure.Data.Configurations;

public sealed class SupplierConfiguration : IEntityTypeConfiguration<Supplier>
{
    public void Configure(EntityTypeBuilder<Supplier> builder)
    {
        builder.ToTable("Suppliers");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.SupplierName)
               .IsRequired()
               .HasMaxLength(200);

        builder.Property(s => s.ContactPerson)
               .HasMaxLength(150);

        builder.Property(s => s.Mobile)
               .HasMaxLength(20);

        builder.Property(s => s.Address)
               .HasMaxLength(500);

        // GSTNumber is optional — not all suppliers are GST-registered.
        builder.Property(s => s.GSTNumber)
               .HasMaxLength(20);
    }
}
