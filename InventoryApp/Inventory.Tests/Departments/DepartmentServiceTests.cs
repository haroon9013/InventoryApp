using AutoMapper;
using Inventory.Core.DTOs.Departments;
using Inventory.Core.Entities;
using Inventory.Core.Mapping;
using Inventory.Core.Services;
using Inventory.Core.Validation.Departments;
using Inventory.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Inventory.Tests.Departments;

public class DepartmentServiceTests
{
    private readonly DbContextOptions<ApplicationDbContext> _dbOptions;
    private readonly IMapper _mapper;

    public DepartmentServiceTests()
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
    public async Task CreateDepartment_Success_SavesDepartmentCorrectly()
    {
        // Arrange
        using var context = CreateContext();
        var service = new DepartmentService(context, _mapper);
        var request = new DepartmentCreateRequest 
        { 
            DepartmentName = "  Bakery  ", // Whitespace test
            Description = "Fresh baked products   " 
        };

        // Act
        var result = await service.CreateAsync(request, 99);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("Bakery", result.Value.DepartmentName); // Trimmed
        Assert.Equal("Fresh baked products", result.Value.Description); // Trimmed
        Assert.True(result.Value.IsActive);

        var dbDept = await context.Departments.SingleOrDefaultAsync(d => d.DepartmentName == "Bakery");
        Assert.NotNull(dbDept);
        Assert.Equal(99, dbDept.CreatedBy);
    }

    [Fact]
    public async Task CreateDepartment_DuplicateDepartmentName_IsRejected()
    {
        // Arrange
        using var context = CreateContext();
        await context.Departments.AddAsync(new Department { DepartmentName = "Kitchen", IsActive = true });
        await context.SaveChangesAsync();

        var service = new DepartmentService(context, _mapper);
        var request = new DepartmentCreateRequest { DepartmentName = "KITCHEN" }; // Case-insensitive duplicate

        // Act
        var result = await service.CreateAsync(request, 99);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("An active department with this name already exists.", result.Error);
    }

    [Fact]
    public async Task CreateDepartment_DuplicateInactiveName_IsAllowed()
    {
        // Arrange
        using var context = CreateContext();
        await context.Departments.AddAsync(new Department { DepartmentName = "Kitchen", IsActive = false });
        await context.SaveChangesAsync();

        var service = new DepartmentService(context, _mapper);
        var request = new DepartmentCreateRequest { DepartmentName = "Kitchen" };

        // Act
        var result = await service.CreateAsync(request, 99);

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task GetAllDepartments_ReturnsAllDepartmentsOrdered()
    {
        // Arrange
        using var context = CreateContext();
        await context.Departments.AddRangeAsync(new[]
        {
            new Department { DepartmentName = "Store", IsActive = true },
            new Department { DepartmentName = "Bar", IsActive = true },
            new Department { DepartmentName = "Bakery", IsActive = true }
        });
        await context.SaveChangesAsync();

        var service = new DepartmentService(context, _mapper);

        // Act
        var result = await service.GetAllAsync();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Value?.Count);
        // Sorted ordering checks
        Assert.Equal("Bakery", result.Value?[0].DepartmentName);
        Assert.Equal("Bar", result.Value?[1].DepartmentName);
        Assert.Equal("Store", result.Value?[2].DepartmentName);
    }

    [Fact]
    public async Task GetDepartmentById_Found_ReturnsDepartment()
    {
        // Arrange
        using var context = CreateContext();
        var dept = new Department { Id = 100, DepartmentName = "Housekeeping", IsActive = true };
        await context.Departments.AddAsync(dept);
        await context.SaveChangesAsync();

        var service = new DepartmentService(context, _mapper);

        // Act
        var result = await service.GetByIdAsync(100);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Housekeeping", result.Value?.DepartmentName);
    }

    [Fact]
    public async Task GetDepartmentById_NotFound_ReturnsFailure()
    {
        // Arrange
        using var context = CreateContext();
        var service = new DepartmentService(context, _mapper);

        // Act
        var result = await service.GetByIdAsync(999);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Department not found.", result.Error);
    }

    [Fact]
    public async Task UpdateDepartment_Success_UpdatesProperties()
    {
        // Arrange
        using var context = CreateContext();
        var dept = new Department { Id = 200, DepartmentName = "Kitchen Staff", IsActive = true };
        await context.Departments.AddAsync(dept);
        await context.SaveChangesAsync();

        var service = new DepartmentService(context, _mapper);
        var request = new DepartmentUpdateRequest { DepartmentName = "Kitchen-Staff", Description = "Cooks", IsActive = true };

        // Act
        var result = await service.UpdateAsync(200, request, 99);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Kitchen-Staff", result.Value?.DepartmentName);
        Assert.Equal("Cooks", result.Value?.Description);
    }

    [Fact]
    public async Task UpdateDepartment_SelfMatch_IsNotRejectedAsDuplicate()
    {
        // Arrange
        using var context = CreateContext();
        var dept = new Department { Id = 300, DepartmentName = "Store", IsActive = true };
        await context.Departments.AddAsync(dept);
        await context.SaveChangesAsync();

        var service = new DepartmentService(context, _mapper);
        var request = new DepartmentUpdateRequest { DepartmentName = "store", Description = "Main store room", IsActive = true }; // Capital check

        // Act
        var result = await service.UpdateAsync(300, request, 99);

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task SoftDeleteDepartment_Success_SetsIsActiveToFalse()
    {
        // Arrange
        using var context = CreateContext();
        var dept = new Department { Id = 400, DepartmentName = "Front Office", IsActive = true };
        await context.Departments.AddAsync(dept);
        await context.SaveChangesAsync();

        var service = new DepartmentService(context, _mapper);

        // Act
        var result = await service.SoftDeleteAsync(400, 99);

        // Assert
        Assert.True(result.IsSuccess);

        var dbDept = await context.Departments.SingleAsync(d => d.Id == 400);
        Assert.False(dbDept.IsActive);
    }

    [Fact]
    public void DepartmentValidators_RejectInvalidInputs()
    {
        // Arrange
        var createValidator = new DepartmentCreateRequestValidator();
        var updateValidator = new DepartmentUpdateRequestValidator();

        var emptyNameRequest = new DepartmentCreateRequest { DepartmentName = "", Description = "Ok" };
        var nameTooLongRequest = new DepartmentCreateRequest { DepartmentName = new string('d', 101) };
        var descTooLongUpdateRequest = new DepartmentUpdateRequest 
        { 
            DepartmentName = "Bakery", 
            Description = new string('s', 501), 
            IsActive = true 
        };

        // Act & Assert
        var emptyValResult = createValidator.Validate(emptyNameRequest);
        Assert.False(emptyValResult.IsValid);
        Assert.Contains(emptyValResult.Errors, e => e.PropertyName == "DepartmentName");

        var longValResult = createValidator.Validate(nameTooLongRequest);
        Assert.False(longValResult.IsValid);
        Assert.Contains(longValResult.Errors, e => e.PropertyName == "DepartmentName");

        var descValResult = updateValidator.Validate(descTooLongUpdateRequest);
        Assert.False(descValResult.IsValid);
        Assert.Contains(descValResult.Errors, e => e.PropertyName == "Description");
    }

    [Fact]
    internal async Task GetDepartments_Filtering_WorksAsExpected()
    {
        // Arrange
        using var context = CreateContext();
        await context.Departments.AddRangeAsync(new[]
        {
            new Department { Id = 10, DepartmentName = "ActiveDept", IsActive = true },
            new Department { Id = 11, DepartmentName = "InactiveDept", IsActive = false }
        });
        await context.SaveChangesAsync();
        var service = new DepartmentService(context, _mapper);

        // Act & Assert 1: Don't include inactive (default)
        var resultActiveOnly = await service.GetAllAsync(includeInactive: false);
        Assert.True(resultActiveOnly.IsSuccess);
        Assert.Single(resultActiveOnly.Value!);
        Assert.Equal("ActiveDept", resultActiveOnly.Value![0].DepartmentName);

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
