using Inventory.Core.DTOs.Auth;
using Inventory.Core.Entities;
using Inventory.Core.Interfaces.Services;
using Inventory.Core.Services;
using Inventory.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace Inventory.Tests.Auth;

public class AuthServiceTests
{
    private readonly DbContextOptions<ApplicationDbContext> _dbOptions;

    public AuthServiceTests()
    {
        _dbOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    private ApplicationDbContext CreateContext() => new(_dbOptions);

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsLoginResponse()
    {
        // Arrange
        using var context = CreateContext();
        var role = new Role { Id = 1, Name = "Admin", IsActive = true };
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword("Password@123");
        var user = new User
        {
            Id = 1,
            FullName = "Admin Test",
            UserName = "admin",
            PasswordHash = hashedPassword,
            RoleId = 1,
            IsActive = true,
            Role = role
        };
        await context.Roles.AddAsync(role);
        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();

        var mockJwtService = new Mock<IJwtService>();
        mockJwtService.Setup(j => j.GenerateToken(It.IsAny<User>(), It.IsAny<string>())).Returns("dummy-token");
        mockJwtService.Setup(j => j.GetExpiryTime()).Returns(DateTime.UtcNow.AddMinutes(60));

        var authService = new AuthService(context, mockJwtService.Object);

        // Act
        var result = await authService.LoginAsync(new LoginRequest { UserName = "admin", Password = "Password@123" });

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("dummy-token", result.Value.AccessToken);
        Assert.Equal("admin", result.Value.User.UserName);
        Assert.Equal("Admin", result.Value.User.Role);
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ReturnsFailureResult()
    {
        // Arrange
        using var context = CreateContext();
        var role = new Role { Id = 1, Name = "Admin", IsActive = true };
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword("Password@123");
        var user = new User
        {
            Id = 1,
            FullName = "Admin Test",
            UserName = "admin",
            PasswordHash = hashedPassword,
            RoleId = 1,
            IsActive = true,
            Role = role
        };
        await context.Roles.AddAsync(role);
        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();

        var mockJwtService = new Mock<IJwtService>();
        var authService = new AuthService(context, mockJwtService.Object);

        // Act
        var result = await authService.LoginAsync(new LoginRequest { UserName = "admin", Password = "WrongPassword" });

        // Assert
        Assert.False(result.IsSuccess);
        // Security requirement: failure output must be generic
        Assert.Equal("Invalid username or password.", result.Error);
    }

    [Fact]
    public async Task Login_WithUnknownUsername_ReturnsFailureResult()
    {
        // Arrange
        using var context = CreateContext();
        var mockJwtService = new Mock<IJwtService>();
        var authService = new AuthService(context, mockJwtService.Object);

        // Act
        var result = await authService.LoginAsync(new LoginRequest { UserName = "unknown", Password = "Password@123" });

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Invalid username or password.", result.Error);
    }

    [Fact]
    public async Task Login_WithInactiveUser_ReturnsFailureResult()
    {
        // Arrange
        using var context = CreateContext();
        var role = new Role { Id = 1, Name = "Admin", IsActive = true };
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword("Password@123");
        var user = new User
        {
            Id = 1,
            FullName = "Admin Test",
            UserName = "admin",
            PasswordHash = hashedPassword,
            RoleId = 1,
            IsActive = false,
            Role = role
        };
        await context.Roles.AddAsync(role);
        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();

        var mockJwtService = new Mock<IJwtService>();
        var authService = new AuthService(context, mockJwtService.Object);

        // Act
        var result = await authService.LoginAsync(new LoginRequest { UserName = "admin", Password = "Password@123" });

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("User account is inactive.", result.Error);
    }
}
