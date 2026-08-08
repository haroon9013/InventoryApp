using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Inventory.Core.Entities;
using Inventory.Core.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Inventory.Infrastructure.Services;

public sealed class JwtService : IJwtService
{
    private readonly IConfiguration _configuration;

    public JwtService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateToken(User user, string roleName)
    {
        var secretKey = _configuration["JwtSettings:SecretKey"] 
            ?? throw new InvalidOperationException("JWT SecretKey configuration is missing.");
        var issuer = _configuration["JwtSettings:Issuer"] ?? "InventoryApp";
        var audience = _configuration["JwtSettings:Audience"] ?? "InventoryApp";
        var expiryMinutesStr = _configuration["JwtSettings:ExpiryMinutes"] ?? "480";
        
        if (!double.TryParse(expiryMinutesStr, out var expiryMinutes))
        {
            expiryMinutes = 480;
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(ClaimTypes.Role, roleName),
            new Claim("FullName", user.FullName)
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public DateTime GetExpiryTime()
    {
        var expiryMinutesStr = _configuration["JwtSettings:ExpiryMinutes"] ?? "480";
        if (!double.TryParse(expiryMinutesStr, out var expiryMinutes))
        {
            expiryMinutes = 480;
        }
        return DateTime.UtcNow.AddMinutes(expiryMinutes);
    }
}
