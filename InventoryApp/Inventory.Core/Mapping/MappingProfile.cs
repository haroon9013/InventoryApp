using AutoMapper;
using Inventory.Core.DTOs.Categories;
using Inventory.Core.DTOs.Departments;
using Inventory.Core.DTOs.Products;
using Inventory.Core.DTOs.Units;
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

        CreateMap<Category, CategoryResponse>();

        CreateMap<Unit, UnitResponse>();

        CreateMap<Department, DepartmentResponse>();

        CreateMap<Product, ProductResponse>()
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.CategoryName : string.Empty))
            .ForMember(dest => dest.UnitName, opt => opt.MapFrom(src => src.Unit != null ? src.Unit.UnitName : string.Empty));
    }
}
