using AutoMapper;
using Inventory.Core.DTOs.Units;
using Inventory.Core.Entities;
using Inventory.Core.Mapping;
using Inventory.Core.Services;
using Inventory.Core.Validation.Units;
using Inventory.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Inventory.Tests.Units;

public class UnitServiceTests
{
    private readonly DbContextOptions<ApplicationDbContext> _dbOptions;
    private readonly IMapper _mapper;

    public UnitServiceTests()
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
    public async Task CreateUnit_Success_SavesUnitCorrectly()
    {
        // Arrange
        using var context = CreateContext();
        var service = new UnitService(context, _mapper);
        var request = new UnitCreateRequest { UnitName = "Kilogram", ShortName = "Kg" };

        // Act
        var result = await service.CreateAsync(request, 99);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("Kilogram", result.Value.UnitName);
        Assert.Equal("Kg", result.Value.ShortName);
        Assert.True(result.Value.IsActive);

        var dbUnit = await context.Units.SingleOrDefaultAsync(u => u.UnitName == "Kilogram");
        Assert.NotNull(dbUnit);
        Assert.Equal(99, dbUnit.CreatedBy);
    }

    [Fact]
    public async Task CreateUnit_DuplicateUnitName_IsRejected()
    {
        // Arrange
        using var context = CreateContext();
        await context.Units.AddAsync(new Unit { UnitName = "Kilogram", ShortName = "Kg", IsActive = true });
        await context.SaveChangesAsync();

        var service = new UnitService(context, _mapper);
        var request = new UnitCreateRequest { UnitName = "kilogram", ShortName = "Kgs" }; // Case-insensitive duplicate check

        // Act
        var result = await service.CreateAsync(request, 99);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("An active unit with this name already exists.", result.Error);
    }

    [Fact]
    public async Task CreateUnit_DuplicateShortName_IsRejected()
    {
        // Arrange
        using var context = CreateContext();
        await context.Units.AddAsync(new Unit { UnitName = "Kilogram", ShortName = "Kg", IsActive = true });
        await context.SaveChangesAsync();

        var service = new UnitService(context, _mapper);
        var request = new UnitCreateRequest { UnitName = "Kilo", ShortName = "kg" }; // Case-insensitive duplicate check

        // Act
        var result = await service.CreateAsync(request, 99);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("An active unit with this short name already exists.", result.Error);
    }

    [Fact]
    public async Task CreateUnit_DuplicateInactiveName_IsAllowed()
    {
        // Arrange
        using var context = CreateContext();
        await context.Units.AddAsync(new Unit { UnitName = "Kilogram", ShortName = "Kg", IsActive = false });
        await context.SaveChangesAsync();

        var service = new UnitService(context, _mapper);
        var request = new UnitCreateRequest { UnitName = "Kilogram", ShortName = "Kg" };

        // Act
        var result = await service.CreateAsync(request, 99);

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task GetAllUnits_ReturnsAllUnits()
    {
        // Arrange
        using var context = CreateContext();
        await context.Units.AddRangeAsync(new[]
        {
            new Unit { UnitName = "Litre", ShortName = "L", IsActive = true },
            new Unit { UnitName = "Gram", ShortName = "g", IsActive = true }
        });
        await context.SaveChangesAsync();

        var service = new UnitService(context, _mapper);

        // Act
        var result = await service.GetAllAsync();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value?.Count);
        // Order check: Gram precedes Litre
        Assert.Equal("Gram", result.Value?[0].UnitName);
    }

    [Fact]
    public async Task GetUnitById_Found_ReturnsUnit()
    {
        // Arrange
        using var context = CreateContext();
        var unit = new Unit { Id = 3, UnitName = "Piece", ShortName = "Pc", IsActive = true };
        await context.Units.AddAsync(unit);
        await context.SaveChangesAsync();

        var service = new UnitService(context, _mapper);

        // Act
        var result = await service.GetByIdAsync(3);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Piece", result.Value?.UnitName);
    }

    [Fact]
    public async Task UpdateUnit_Success_UpdatesUnitProperties()
    {
        // Arrange
        using var context = CreateContext();
        var unit = new Unit { Id = 1, UnitName = "Millilitre", ShortName = "ml", IsActive = true };
        await context.Units.AddAsync(unit);
        await context.SaveChangesAsync();

        var service = new UnitService(context, _mapper);
        var request = new UnitUpdateRequest { UnitName = "Milli-Litre", ShortName = "mL", IsActive = true };

        // Act
        var result = await service.UpdateAsync(1, request, 99);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Milli-Litre", result.Value?.UnitName);
        Assert.Equal("mL", result.Value?.ShortName);
    }

    [Fact]
    public async Task UpdateUnit_SelfMatch_IsNotRejectedAsDuplicate()
    {
        // Arrange
        using var context = CreateContext();
        var unit = new Unit { Id = 2, UnitName = "Millilitre", ShortName = "ml", IsActive = true };
        await context.Units.AddAsync(unit);
        await context.SaveChangesAsync();

        var service = new UnitService(context, _mapper);
        // Updating description/casing or keeping identical details should not prompt duplicate failure
        var request = new UnitUpdateRequest { UnitName = "Millilitre", ShortName = "ml", IsActive = true };

        // Act
        var result = await service.UpdateAsync(2, request, 99);

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task SoftDeleteUnit_Success_SetsIsActiveToFalse()
    {
        // Arrange
        using var context = CreateContext();
        var unit = new Unit { Id = 10, UnitName = "Box", ShortName = "Bx", IsActive = true };
        await context.Units.AddAsync(unit);
        await context.SaveChangesAsync();

        var service = new UnitService(context, _mapper);

        // Act
        var result = await service.SoftDeleteAsync(10, 99);

        // Assert
        Assert.True(result.IsSuccess);

        var dbUnit = await context.Units.SingleAsync(u => u.Id == 10);
        Assert.False(dbUnit.IsActive); // Soft deleted is inactive
    }

    [Fact]
    public void UnitValidators_RejectInvalidInputs()
    {
        // Arrange
        var createValidator = new UnitCreateRequestValidator();
        var updateValidator = new UnitUpdateRequestValidator();

        var badCreateRequest = new UnitCreateRequest { UnitName = "", ShortName = "Kg" }; // empty name
        var longShortNameCreateRequest = new UnitCreateRequest 
        { 
            UnitName = "Kilogram", 
            ShortName = new string('s', 21) // exceeds 20 characters limit
        };

        // Act & Assert
        var createValidationResult = createValidator.Validate(badCreateRequest);
        Assert.False(createValidationResult.IsValid);
        Assert.Contains(createValidationResult.Errors, e => e.PropertyName == "UnitName");

        var shortNameValidationResult = createValidator.Validate(longShortNameCreateRequest);
        Assert.False(shortNameValidationResult.IsValid);
        Assert.Contains(shortNameValidationResult.Errors, e => e.PropertyName == "ShortName");

        var badUpdateRequest = new UnitUpdateRequest { UnitName = new string('x', 51), ShortName = "ok", IsActive = true }; // exceeds 50 limit
        var updateValidationResult = updateValidator.Validate(badUpdateRequest);
        Assert.False(updateValidationResult.IsValid);
        Assert.Contains(updateValidationResult.Errors, e => e.PropertyName == "UnitName");
    }

    [Fact]
    internal async Task GetUnits_Filtering_WorksAsExpected()
    {
        // Arrange
        using var context = CreateContext();
        await context.Units.AddRangeAsync(new[]
        {
            new Unit { Id = 10, UnitName = "ActiveUnit", ShortName = "AU", IsActive = true },
            new Unit { Id = 11, UnitName = "InactiveUnit", ShortName = "IU", IsActive = false }
        });
        await context.SaveChangesAsync();
        var service = new UnitService(context, _mapper);

        // Act & Assert 1: Don't include inactive (default)
        var resultActiveOnly = await service.GetAllAsync(includeInactive: false);
        Assert.True(resultActiveOnly.IsSuccess);
        Assert.Single(resultActiveOnly.Value!);
        Assert.Equal("ActiveUnit", resultActiveOnly.Value![0].UnitName);

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
