using AutoMapper;

namespace Inventory.Core.Mapping;

/// <summary>
/// Root AutoMapper profile.
/// Module-specific profiles will be added in sub-classes as modules are implemented.
/// </summary>
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Profiles will be added here as each module is implemented.
        // Example: CreateMap<Product, ProductDto>().ReverseMap();
    }
}
