using Inventory.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Inventory.Infrastructure.Extensions;

/// <summary>
/// Registers all Infrastructure-layer services into the DI container.
/// Called from the API's Program.cs via builder.Services.AddInfrastructure().
/// </summary>
public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ── Database ──────────────────────────────────────────────────────
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sqlOptions =>
                {
                    sqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                    sqlOptions.EnableRetryOnFailure(maxRetryCount: 3);
                }));

        // Register DbContext as interface so Core services can use it without
        // taking a direct dependency on the Infrastructure assembly.
        services.AddScoped<Core.Interfaces.IApplicationDbContext>(
            sp => sp.GetRequiredService<ApplicationDbContext>());

        // ── Generic Repository (open generic registration) ─────────────
        services.AddScoped(typeof(Core.Interfaces.Repositories.IRepository<>),
                           typeof(Repositories.Repository<>));

        return services;
    }
}
