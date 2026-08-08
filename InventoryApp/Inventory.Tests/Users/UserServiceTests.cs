using AutoMapper;
using Inventory.Core.DTOs.Users;
using Inventory.Core.Entities;
using Inventory.Core.Mapping;
using Inventory.Core.Services;
using Inventory.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Inventory.Tests.Users;

public class UserServiceTests
{
    private readonly DbContextOptions<ApplicationDbContext> _dbOptions;
    private readonly IMapper _mapper;

    public UserServiceTests()
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
    public async Task CreateUser_HashesPasswordAndDoesNotExposeHashInResponse()
    {
        // Arrange
        using var context = CreateContext();
        var role = new Role { Id = 1, Name = "Admin", IsActive = true };
        await context.Roles.AddAsync(role);
        await context.SaveChangesAsync();

        var userService = new UserService(context, _mapper);

        var request = new UserCreateRequest
        {
            FullName = "New User",
            UserName = "newuser",
            Password = "SecurePassword@123",
            RoleId = 1,
            IsActive = true
        };

        // Act
        var result = await userService.CreateAsync(request, 99);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("New User", result.Value.FullName);
        Assert.Equal("newuser", result.Value.UserName);

        // Security check: UserResponse does not contain password hash field or password hash value.
        // It must NOT expose private fields.
        var userType = typeof(UserResponse);
        var properties = userType.GetProperties().Select(p => p.Name).ToList();
        Assert.DoesNotContain("PasswordHash", properties);
        Assert.DoesNotContain("Password", properties);

        // Verify database entry has hashed password
        var dbUser = await context.Users.SingleOrDefaultAsync(u => u.UserName == "newuser");
        Assert.NotNull(dbUser);
        Assert.NotEqual("SecurePassword@123", dbUser.PasswordHash);
        Assert.True(BCrypt.Net.BCrypt.Verify("SecurePassword@123", dbUser.PasswordHash));
    }

    [Fact]
    public async Task UserChangePassword_WithCorrectPassword_ChangesPasswordSuccessfully()
    {
        // Arrange
        using var context = CreateContext();
        var role = new Role { Id = 3, Name = "KitchenUser", IsActive = true };
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword("OldPassword@123");
        var user = new User
        {
            Id = 10,
            FullName = "Chef Bob",
            UserName = "chefbob",
            PasswordHash = hashedPassword,
            RoleId = 3,
            IsActive = true,
            Role = role
        };
        await context.Roles.AddAsync(role);
        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();

        var userService = new UserService(context, _mapper);

        var request = new ChangePasswordRequest
        {
            CurrentPassword = "OldPassword@123",
            NewPassword = "NewSecretPassword@1"
        };

        // Act
        var result = await userService.ChangePasswordAsync(10, request);

        // Assert
        Assert.True(result.IsSuccess);

        // Verify password hash updated in Db
        var dbUser = await context.Users.SingleAsync(u => u.Id == 10);
        Assert.True(BCrypt.Net.BCrypt.Verify("NewSecretPassword@1", dbUser.PasswordHash));
        Assert.False(BCrypt.Net.BCrypt.Verify("OldPassword@123", dbUser.PasswordHash));
    }

    [Fact]
    public async Task UserChangePassword_WithIncorrectCurrentPassword_ReturnsFailureResult()
    {
        // Arrange
        using var context = CreateContext();
        var role = new Role { Id = 3, Name = "KitchenUser", IsActive = true };
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword("OldPassword@123");
        var user = new User
        {
            Id = 10,
            FullName = "Chef Bob",
            UserName = "chefbob",
            PasswordHash = hashedPassword,
            RoleId = 3,
            IsActive = true,
            Role = role
        };
        await context.Roles.AddAsync(role);
        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();

        var userService = new UserService(context, _mapper);

        var request = new ChangePasswordRequest
        {
            CurrentPassword = "WrongPassword",
            NewPassword = "NewSecretPassword@1"
        };

        // Act
        var result = await userService.ChangePasswordAsync(10, request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Current password is incorrect.", result.Error);
    }
}
