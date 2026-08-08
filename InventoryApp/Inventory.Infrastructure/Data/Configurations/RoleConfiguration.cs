using Inventory.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventory.Infrastructure.Data.Configurations;

public sealed class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Roles");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Name)
               .IsRequired()
               .HasMaxLength(50);

        builder.HasIndex(r => r.Name)
               .IsUnique()
               .HasDatabaseName("IX_Roles_Name");

        // ── Seed Data ─────────────────────────────────────────────────────
        builder.HasData(
            new Role { Id = 1, Name = "Admin",       IsActive = true },
            new Role { Id = 2, Name = "StoreKeeper", IsActive = true },
            new Role { Id = 3, Name = "KitchenUser", IsActive = true }
        );
    }
}
