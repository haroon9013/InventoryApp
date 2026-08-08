using Inventory.Core.DTOs.Auth;
using Inventory.Core.Entities;
using Inventory.Core.Interfaces;
using Inventory.Core.Interfaces.Services;
using Inventory.Shared.Common;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Core.Services;

public sealed class AuthService : IAuthService
{
    private readonly IApplicationDbContext _context;
    private readonly IJwtService _jwtService;

    public AuthService(IApplicationDbContext context, IJwtService jwtService)
    {
        _context = context;
        _jwtService = jwtService;
    }

    public async Task<Result<LoginResponse>> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var user = await _context.Users
            .Include(u => u.Role)
            .SingleOrDefaultAsync(u => u.UserName == request.UserName, ct);

        // Security requirement: Authentication failures must not reveal which credential was incorrect.
        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return Result<LoginResponse>.Failure("Invalid username or password.");
        }

        // Security requirement: Inactive users cannot log in.
        if (!user.IsActive)
        {
            return Result<LoginResponse>.Failure("User account is inactive.");
        }

        var roleName = user.Role?.Name ?? "KitchenUser";
        var token = _jwtService.GenerateToken(user, roleName);
        var expiresAt = _jwtService.GetExpiryTime();

        var response = new LoginResponse
        {
            AccessToken = token,
            ExpiresAt = expiresAt,
            User = new UserInfo
            {
                Id = user.Id,
                FullName = user.FullName,
                UserName = user.UserName,
                Role = roleName
            }
        };

        return Result<LoginResponse>.Success(response);
    }
}
