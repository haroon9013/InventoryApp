using Inventory.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventory.Infrastructure.Data.Configurations;

public sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.CategoryName)
               .IsRequired()
               .HasMaxLength(100);

        builder.Property(c => c.Description)
               .HasMaxLength(250);

        // Filtered unique index — allows duplicate names across inactive records
        // (e.g. a category can be recreated after being deactivated).
        builder.HasIndex(c => c.CategoryName)
               .IsUnique()
               .HasFilter("[IsActive] = 1")
               .HasDatabaseName("IX_Categories_CategoryName_Active");
    }
}
