using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Inventory.Core.Entities;
using Inventory.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace Inventory.Tests.Auth;

public class JwtServiceTests
{
    [Fact]
    public void GenerateToken_WithValidInput_CreatesSignedTokenWithCorrectClaims()
    {
        // Arrange
        var mockConfig = new Mock<IConfiguration>();
        // Secret key must be at least 256 bits (32 bytes)
        mockConfig.Setup(c => c["JwtSettings:SecretKey"]).Returns("DevelopmentSecretKeyAtLeast32CharsLong1234567890!");
        mockConfig.Setup(c => c["JwtSettings:Issuer"]).Returns("InventoryTestIssuer");
        mockConfig.Setup(c => c["JwtSettings:Audience"]).Returns("InventoryTestAudience");
        mockConfig.Setup(c => c["JwtSettings:ExpiryMinutes"]).Returns("60");

        var jwtService = new JwtService(mockConfig.Object);

        var user = new User
        {
            Id = 42,
            UserName = "johndoe",
            FullName = "John Doe"
        };

        // Act
        var tokenString = jwtService.GenerateToken(user, "StoreKeeper");

        // Assert
        Assert.NotNull(tokenString);

        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadJwtToken(tokenString);

        Assert.Equal("InventoryTestIssuer", jsonToken.Issuer);
        Assert.Contains("InventoryTestAudience", jsonToken.Audiences);

        // Security requirement: JWT claims userId, username, role, fullname
        var claims = jsonToken.Claims.ToDictionary(c => c.Type, c => c.Value);

        Assert.Contains(ClaimTypes.NameIdentifier, claims.Keys);
        Assert.Equal("42", claims[ClaimTypes.NameIdentifier]);

        Assert.Contains(ClaimTypes.Name, claims.Keys);
        Assert.Equal("johndoe", claims[ClaimTypes.Name]);

        Assert.Contains(ClaimTypes.Role, claims.Keys);
        Assert.Equal("StoreKeeper", claims[ClaimTypes.Role]);

        Assert.Contains("FullName", claims.Keys);
        Assert.Equal("John Doe", claims["FullName"]);
    }
}
