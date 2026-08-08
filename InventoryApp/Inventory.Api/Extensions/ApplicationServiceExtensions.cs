using FluentValidation;
using FluentValidation.AspNetCore;
using Inventory.Core.Mapping;
using Microsoft.Extensions.DependencyInjection;

namespace Inventory.Api.Extensions;

/// <summary>
/// Registers application (Core) services into the DI container.
/// AutoMapper and FluentValidation scan the Core assembly for profiles/validators.
/// </summary>
public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // AutoMapper — scans Core assembly for MappingProfile classes.
        services.AddAutoMapper(typeof(MappingProfile).Assembly);

        // FluentValidation — scans Core assembly for IValidator<T> implementations.
        services.AddValidatorsFromAssembly(typeof(MappingProfile).Assembly);

        // Application Modules
        services.AddScoped<Inventory.Core.Interfaces.Services.IJwtService, Inventory.Infrastructure.Services.JwtService>();
        services.AddScoped<Inventory.Core.Interfaces.Services.IAuthService, Inventory.Core.Services.AuthService>();
        services.AddScoped<Inventory.Core.Interfaces.Services.IUserService, Inventory.Core.Services.UserService>();
        services.AddScoped<Inventory.Core.Interfaces.Services.ICategoryService, Inventory.Core.Services.CategoryService>();
        services.AddScoped<Inventory.Core.Interfaces.Services.IUnitService, Inventory.Core.Services.UnitService>();

        return services;
    }
}
