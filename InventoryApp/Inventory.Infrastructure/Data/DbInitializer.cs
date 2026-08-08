using Inventory.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Inventory.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();

        // Apply migrations automatically on startup if any pending
        var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
        if (pendingMigrations.Any())
        {
            logger.LogInformation("Applying pending migrations...");
            await context.Database.MigrateAsync();
        }

        // Seed initial admin user if no users exist
        if (!await context.Users.AnyAsync())
        {
            var adminPassword = configuration["Initialization:AdminPassword"];
            if (string.IsNullOrWhiteSpace(adminPassword))
            {
                logger.LogWarning("───────────────────────────────────────────────────────────────────────────");
                logger.LogWarning("WARNING: No users found in database, and 'Initialization:AdminPassword' is not");
                logger.LogWarning("configured in appsettings.json. No default admin user created.");
                logger.LogWarning("Please configure a password to allow automatic first-time admin creation.");
                logger.LogWarning("───────────────────────────────────────────────────────────────────────────");
                return;
            }

            // Ensure Roles are available (they are seeded in migration, but we verify)
            var adminRoleExists = await context.Roles.AnyAsync(r => r.Id == 1);
            if (!adminRoleExists)
            {
                logger.LogError("Roles not seeded in the database. Ensure migration seeding has ran.");
                return;
            }

            var adminUser = new User
            {
                FullName = "System Administrator",
                UserName = "admin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(adminPassword),
                RoleId = 1, // Admin role
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await context.Users.AddAsync(adminUser);
            await context.SaveChangesAsync();

            logger.LogInformation("Default Admin user ('admin') successfully initialized using configured password.");
        }
    }
}
