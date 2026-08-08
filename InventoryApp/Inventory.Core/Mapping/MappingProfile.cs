using AutoMapper;
using Inventory.Core.DTOs.Users;
using Inventory.Core.Entities;

namespace Inventory.Core.Mapping;

/// <summary>
/// Root AutoMapper profile.
/// Module-specific profiles will be added in sub-classes as modules are implemented.
/// </summary>
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<User, UserResponse>()
            .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.Role != null ? src.Role.Name : string.Empty));
    }
}
