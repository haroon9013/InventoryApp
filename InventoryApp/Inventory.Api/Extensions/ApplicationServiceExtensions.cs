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
        // Registered in DI for manual injection; pipeline integration is added via
        // FluentValidation.AspNetCore in the controller configuration.
        services.AddValidatorsFromAssembly(typeof(MappingProfile).Assembly);

        return services;
    }
}
