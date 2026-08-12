using AutoMapper;
using Inventory.Core.DTOs.Products;
using Inventory.Core.Entities;
using Inventory.Core.Mapping;
using Inventory.Core.Services;
using Inventory.Core.Validation.Products;
using Inventory.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Inventory.Tests.Products;

public class ProductServiceTests
{
    private readonly DbContextOptions<ApplicationDbContext> _dbOptions;
    private readonly IMapper _mapper;

    public ProductServiceTests()
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

    private async Task SeedBaseResourcesAsync(ApplicationDbContext context)
    {
        await context.Categories.AddRangeAsync(new[]
        {
            new Category { Id = 1, CategoryName = "Active Category", IsActive = true },
            new Category { Id = 2, CategoryName = "Inactive Category", IsActive = false }
        });

        await context.Units.AddRangeAsync(new[]
        {
            new Unit { Id = 1, UnitName = "Active Unit", ShortName = "AU", IsActive = true },
            new Unit { Id = 2, UnitName = "Inactive Unit", ShortName = "IU", IsActive = false }
        });

        await context.SaveChangesAsync();
    }

    [Fact]
    public async Task CreateProduct_Success_SavesProductCorrectly_WithTrimmedAndUppercaseCode()
    {
        // Arrange
        using var context = CreateContext();
        await SeedBaseResourcesAsync(context);

        var service = new ProductService(context, _mapper);
        var request = new ProductCreateRequest 
        { 
            ProductCode = "  prd-101   ", // Whitespace and lowercase
            ProductName = "  Product One   ", 
            CategoryId = 1, 
            UnitId = 1, 
            MinimumStock = 10m 
        };

        // Act
        var result = await service.CreateAsync(request, 99);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("PRD-101", result.Value.ProductCode); // Normalized
        Assert.Equal("Product One", result.Value.ProductName); // Trimmed
        Assert.Equal(0m, result.Value.CurrentStock);
        Assert.Equal(0m, result.Value.LastPurchasePrice);
        Assert.Equal("Active Category", result.Value.CategoryName);
        Assert.Equal("Active Unit", result.Value.UnitName);
    }

    [Fact]
    public async Task CreateProduct_DuplicateProductCode_IsRejected_AmongActiveAndInactiveProducts()
    {
        // Arrange
        using var context = CreateContext();
        await SeedBaseResourcesAsync(context);

        // Seed an inactive product with code PRD-999
        await context.Products.AddAsync(new Product 
        { 
            ProductCode = "PRD-999", 
            ProductName = "Old Product", 
            CategoryId = 1, 
            UnitId = 1, 
            IsActive = false 
        });
        await context.SaveChangesAsync();

        var service = new ProductService(context, _mapper);
        var request = new ProductCreateRequest 
        { 
            ProductCode = "prd-999", // Case-insensitive duplicate check
            ProductName = "New Product", 
            CategoryId = 1, 
            UnitId = 1 
        };

        // Act
        var result = await service.CreateAsync(request, 99);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("A product with this product code already exists.", result.Error);
    }

    [Fact]
    public async Task CreateProduct_InactiveCategory_ReturnsFailure()
    {
        // Arrange
        using var context = CreateContext();
        await SeedBaseResourcesAsync(context);

        var service = new ProductService(context, _mapper);
        var request = new ProductCreateRequest 
        { 
            ProductCode = "PRD-102", 
            ProductName = "Product Two", 
            CategoryId = 2, // Inactive category
            UnitId = 1 
        };

        // Act
        var result = await service.CreateAsync(request, 99);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Category not found or is inactive.", result.Error);
    }

    [Fact]
    public async Task CreateProduct_InactiveUnit_ReturnsFailure()
    {
        // Arrange
        using var context = CreateContext();
        await SeedBaseResourcesAsync(context);

        var service = new ProductService(context, _mapper);
        var request = new ProductCreateRequest 
        { 
            ProductCode = "PRD-102", 
            ProductName = "Product Two", 
            CategoryId = 1, 
            UnitId = 2 // Inactive unit
        };

        // Act
        var result = await service.CreateAsync(request, 99);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Unit not found or is inactive.", result.Error);
    }

    [Fact]
    public async Task GetAllProducts_ReturnsAllActiveProductsOrdered_ByDefault()
    {
        // Arrange
        using var context = CreateContext();
        await SeedBaseResourcesAsync(context);
        await context.Products.AddRangeAsync(new[]
        {
            new Product { ProductCode = "P3", ProductName = "Zeta Product", CategoryId = 1, UnitId = 1, IsActive = true },
            new Product { ProductCode = "P4", ProductName = "Alpha Product", CategoryId = 1, UnitId = 1, IsActive = true },
            new Product { ProductCode = "P5", ProductName = "Inactive Product", CategoryId = 1, UnitId = 1, IsActive = false }
        });
        await context.SaveChangesAsync();

        var service = new ProductService(context, _mapper);

        // Act
        var result = await service.GetAllAsync(includeInactive: false);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value?.Count);
        Assert.Equal("Alpha Product", result.Value?[0].ProductName); // Alphabetical sort check
        Assert.Equal("Zeta Product", result.Value?[1].ProductName);
    }

    [Fact]
    public async Task GetAllProducts_IncludesInactiveProducts_WhenIncludeInactiveIsTrue()
    {
        // Arrange
        using var context = CreateContext();
        await SeedBaseResourcesAsync(context);
        await context.Products.AddRangeAsync(new[]
        {
            new Product { ProductCode = "P3", ProductName = "Active Product", CategoryId = 1, UnitId = 1, IsActive = true },
            new Product { ProductCode = "P4", ProductName = "Inactive Product", CategoryId = 1, UnitId = 1, IsActive = false }
        });
        await context.SaveChangesAsync();

        var service = new ProductService(context, _mapper);

        // Act
        var result = await service.GetAllAsync(includeInactive: true);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value?.Count);
    }

    [Fact]
    public async Task GetProductById_Active_ReturnsSuccess()
    {
        // Arrange
        using var context = CreateContext();
        await SeedBaseResourcesAsync(context);
        var product = new Product { Id = 10, ProductCode = "PRD-10", ProductName = "Active Product", CategoryId = 1, UnitId = 1, IsActive = true };
        await context.Products.AddAsync(product);
        await context.SaveChangesAsync();

        var service = new ProductService(context, _mapper);

        // Act
        var result = await service.GetByIdAsync(10, includeInactive: false);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("PRD-10", result.Value?.ProductCode);
    }

    [Fact]
    public async Task GetProductById_Inactive_ReturnsFailure_ForNormalUsers()
    {
        // Arrange
        using var context = CreateContext();
        await SeedBaseResourcesAsync(context);
        var product = new Product { Id = 10, ProductCode = "PRD-10", ProductName = "Inactive Product", CategoryId = 1, UnitId = 1, IsActive = false };
        await context.Products.AddAsync(product);
        await context.SaveChangesAsync();

        var service = new ProductService(context, _mapper);

        // Act
        var result = await service.GetByIdAsync(10, includeInactive: false);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Product not found.", result.Error);
    }

    [Fact]
    public async Task GetProductById_Inactive_ReturnsSuccess_ForAdmins()
    {
        // Arrange
        using var context = CreateContext();
        await SeedBaseResourcesAsync(context);
        var product = new Product { Id = 10, ProductCode = "PRD-10", ProductName = "Inactive Product", CategoryId = 1, UnitId = 1, IsActive = false };
        await context.Products.AddAsync(product);
        await context.SaveChangesAsync();

        var service = new ProductService(context, _mapper);

        // Act
        var result = await service.GetByIdAsync(10, includeInactive: true);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("PRD-10", result.Value?.ProductCode);
    }

    [Fact]
    public async Task UpdateProduct_Success_UpdatesProductProperties()
    {
        // Arrange
        using var context = CreateContext();
        await SeedBaseResourcesAsync(context);
        var product = new Product { Id = 20, ProductCode = "PRD-20", ProductName = "Old Name", CategoryId = 1, UnitId = 1, IsActive = true };
        await context.Products.AddAsync(product);
        await context.SaveChangesAsync();

        var service = new ProductService(context, _mapper);
        var request = new ProductUpdateRequest { ProductCode = "prd-updated", ProductName = "New Name", CategoryId = 1, UnitId = 1, MinimumStock = 5m, IsActive = true };

        // Act
        var result = await service.UpdateAsync(20, request, 99);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("PRD-UPDATED", result.Value?.ProductCode);
        Assert.Equal("New Name", result.Value?.ProductName);
        Assert.Equal(5m, result.Value?.MinimumStock);
    }

    [Fact]
    public async Task UpdateProduct_SelfMatch_IsNotRejectedAsDuplicate()
    {
        // Arrange
        using var context = CreateContext();
        await SeedBaseResourcesAsync(context);
        var product = new Product { Id = 30, ProductCode = "PRD-30", ProductName = "Some Product", CategoryId = 1, UnitId = 1, IsActive = true };
        await context.Products.AddAsync(product);
        await context.SaveChangesAsync();

        var service = new ProductService(context, _mapper);
        var request = new ProductUpdateRequest { ProductCode = "PRD-30", ProductName = "Some Product", CategoryId = 1, UnitId = 1, MinimumStock = 0m, IsActive = true };

        // Act
        var result = await service.UpdateAsync(30, request, 99);

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task SoftDeleteProduct_Success_SetsIsActiveToFalse()
    {
        // Arrange
        using var context = CreateContext();
        await SeedBaseResourcesAsync(context);
        var product = new Product { Id = 40, ProductCode = "PRD-40", ProductName = "Test Product", CategoryId = 1, UnitId = 1, IsActive = true };
        await context.Products.AddAsync(product);
        await context.SaveChangesAsync();

        var service = new ProductService(context, _mapper);

        // Act
        var result = await service.SoftDeleteAsync(40, 99);

        // Assert
        Assert.True(result.IsSuccess);

        var dbProduct = await context.Products.SingleAsync(p => p.Id == 40);
        Assert.False(dbProduct.IsActive);
    }

    [Fact]
    public void ProductValidators_ValidateCorrectly()
    {
        // Arrange
        var createValidator = new ProductCreateRequestValidator();
        var updateValidator = new ProductUpdateRequestValidator();

        var badCreateRequest = new ProductCreateRequest { ProductCode = "", ProductName = "Test", CategoryId = 0, UnitId = 1, MinimumStock = -2m };

        // Act & Assert
        var validationResult = createValidator.Validate(badCreateRequest);
        Assert.False(validationResult.IsValid);
        Assert.Contains(validationResult.Errors, e => e.PropertyName == "ProductCode");
        Assert.Contains(validationResult.Errors, e => e.PropertyName == "CategoryId");
        Assert.Contains(validationResult.Errors, e => e.PropertyName == "MinimumStock");
    }
}
