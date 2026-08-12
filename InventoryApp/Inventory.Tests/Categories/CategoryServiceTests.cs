using AutoMapper;
using Inventory.Core.DTOs.Categories;
using Inventory.Core.Entities;
using Inventory.Core.Mapping;
using Inventory.Core.Services;
using Inventory.Core.Validation.Categories;
using Inventory.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Inventory.Tests.Categories;

public class CategoryServiceTests
{
    private readonly DbContextOptions<ApplicationDbContext> _dbOptions;
    private readonly IMapper _mapper;

    public CategoryServiceTests()
    {
        _dbOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile(new MappingProfile());
        });
        _mapper = mapperConfig.CreateMapper();
    }

    private ApplicationDbContext CreateContext() => new(_dbOptions);

    [Fact]
    public async Task CreateCategory_Success_SavesCategoryCorrectly()
    {
        // Arrange
        using var context = CreateContext();
        var service = new CategoryService(context, _mapper);
        var request = new CategoryCreateRequest { CategoryName = "Beverages", Description = "Drinks and sodas" };

        // Act
        var result = await service.CreateAsync(request, 99);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("Beverages", result.Value.CategoryName);
        Assert.Equal("Drinks and sodas", result.Value.Description);
        Assert.True(result.Value.IsActive);

        var dbCategory = await context.Categories.SingleOrDefaultAsync(c => c.CategoryName == "Beverages");
        Assert.NotNull(dbCategory);
        Assert.Equal(99, dbCategory.CreatedBy);
    }

    [Fact]
    public async Task CreateCategory_DuplicateActiveName_IsRejected()
    {
        // Arrange
        using var context = CreateContext();
        await context.Categories.AddAsync(new Category { CategoryName = "Beverages", IsActive = true });
        await context.SaveChangesAsync();

        var service = new CategoryService(context, _mapper);
        var request = new CategoryCreateRequest { CategoryName = "beverages" }; // Case-insensitive duplicate checking

        // Act
        var result = await service.CreateAsync(request, 99);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("An active category with this name already exists.", result.Error);
    }

    [Fact]
    public async Task CreateCategory_DuplicateInactiveName_IsAllowed()
    {
        // Arrange
        using var context = CreateContext();
        await context.Categories.AddAsync(new Category { CategoryName = "Beverages", IsActive = false });
        await context.SaveChangesAsync();

        var service = new CategoryService(context, _mapper);
        var request = new CategoryCreateRequest { CategoryName = "Beverages" };

        // Act
        var result = await service.CreateAsync(request, 99);

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task GetCategories_ReturnsAllCategories()
    {
        // Arrange
        using var context = CreateContext();
        await context.Categories.AddRangeAsync(new[]
        {
            new Category { CategoryName = "Grains", IsActive = true },
            new Category { CategoryName = "Dairy", IsActive = true }
        });
        await context.SaveChangesAsync();

        var service = new CategoryService(context, _mapper);

        // Act
        var result = await service.GetAllAsync();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value?.Count);
        // Order by CategoryName check: Dairy should precede Grains
        Assert.Equal("Dairy", result.Value?[0].CategoryName);
    }

    [Fact]
    public async Task GetCategoryById_Found_ReturnsCategory()
    {
        // Arrange
        using var context = CreateContext();
        var cat = new Category { Id = 5, CategoryName = "Bakery", IsActive = true };
        await context.Categories.AddAsync(cat);
        await context.SaveChangesAsync();

        var service = new CategoryService(context, _mapper);

        // Act
        var result = await service.GetByIdAsync(5);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Bakery", result.Value?.CategoryName);
    }

    [Fact]
    public async Task GetCategoryById_NotFound_ReturnsFailure()
    {
        // Arrange
        using var context = CreateContext();
        var service = new CategoryService(context, _mapper);

        // Act
        var result = await service.GetByIdAsync(999);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Category not found.", result.Error);
    }

    [Fact]
    public async Task UpdateCategory_Success_UpdatesCategoryProperties()
    {
        // Arrange
        using var context = CreateContext();
        var cat = new Category { Id = 1, CategoryName = "Fruits", IsActive = true };
        await context.Categories.AddAsync(cat);
        await context.SaveChangesAsync();

        var service = new CategoryService(context, _mapper);
        var request = new CategoryUpdateRequest { CategoryName = "Fresh Fruits", Description = "Oranges, apples, pears", IsActive = true };

        // Act
        var result = await service.UpdateAsync(1, request, 99);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Fresh Fruits", result.Value?.CategoryName);
        Assert.Equal("Oranges, apples, pears", result.Value?.Description);
        Assert.Equal(99, context.Categories.Single(c => c.Id == 1).UpdatedBy);
    }

    [Fact]
    public async Task SoftDeleteCategory_Success_SetsIsActiveToFalse()
    {
        // Arrange
        using var context = CreateContext();
        var cat = new Category { Id = 2, CategoryName = "Vegetables", IsActive = true };
        await context.Categories.AddAsync(cat);
        await context.SaveChangesAsync();

        var service = new CategoryService(context, _mapper);

        // Act
        var result = await service.SoftDeleteAsync(2, 99);

        // Assert
        Assert.True(result.IsSuccess);

        var dbCategory = await context.Categories.SingleAsync(c => c.Id == 2);
        Assert.False(dbCategory.IsActive); // Soft deleted is inactive
        Assert.Equal(99, dbCategory.UpdatedBy);
    }

    [Fact]
    public void Validators_RejectInvalidInputs()
    {
        // Arrange
        var createValidator = new CategoryCreateRequestValidator();
        var updateValidator = new CategoryUpdateRequestValidator();

        var badCreateRequest = new CategoryCreateRequest { CategoryName = "" }; // empty name
        var longDescriptionCreateRequest = new CategoryCreateRequest 
        { 
            CategoryName = "valid", 
            Description = new string('a', 501) // exceeds 500 chars limit
        };

        // Act & Assert
        var createValidationResult = createValidator.Validate(badCreateRequest);
        Assert.False(createValidationResult.IsValid);
        Assert.Contains(createValidationResult.Errors, e => e.PropertyName == "CategoryName");

        var descValidationResult = createValidator.Validate(longDescriptionCreateRequest);
        Assert.False(descValidationResult.IsValid);
        Assert.Contains(descValidationResult.Errors, e => e.PropertyName == "Description");

        var badUpdateRequest = new CategoryUpdateRequest { CategoryName = new string('x', 101) }; // exceeds 100 limit
        var updateValidationResult = updateValidator.Validate(badUpdateRequest);
        Assert.False(updateValidationResult.IsValid);
        Assert.Contains(updateValidationResult.Errors, e => e.PropertyName == "CategoryName");
    }

    [Fact]
    internal async Task GetCategories_Filtering_WorksAsExpected()
    {
        // Arrange
        using var context = CreateContext();
        await context.Categories.AddRangeAsync(new[]
        {
            new Category { Id = 10, CategoryName = "ActiveCategory", IsActive = true },
            new Category { Id = 11, CategoryName = "InactiveCategory", IsActive = false }
        });
        await context.SaveChangesAsync();
        var service = new CategoryService(context, _mapper);

        // Act & Assert 1: Don't include inactive (default)
        var resultActiveOnly = await service.GetAllAsync(includeInactive: false);
        Assert.True(resultActiveOnly.IsSuccess);
        Assert.Single(resultActiveOnly.Value!);
        Assert.Equal("ActiveCategory", resultActiveOnly.Value![0].CategoryName);

        // Act & Assert 2: Include inactive
        var resultAll = await service.GetAllAsync(includeInactive: true);
        Assert.True(resultAll.IsSuccess);
        Assert.Equal(2, resultAll.Value!.Count);

        // Act & Assert 3: GetById active
        var activeGet = await service.GetByIdAsync(10, includeInactive: false);
        Assert.True(activeGet.IsSuccess);

        // Act & Assert 4: GetById inactive with includeInactive = false (should fail)
        var inactiveGetFail = await service.GetByIdAsync(11, includeInactive: false);
        Assert.False(inactiveGetFail.IsSuccess);

        // Act & Assert 5: GetById inactive with includeInactive = true (should succeed)
        var inactiveGetSucceed = await service.GetByIdAsync(11, includeInactive: true);
        Assert.True(inactiveGetSucceed.IsSuccess);
    }
}
