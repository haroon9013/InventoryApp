using AutoMapper;
using Inventory.Core.DTOs.Users;
using Inventory.Core.Entities;
using Inventory.Core.Interfaces;
using Inventory.Core.Interfaces.Services;
using Inventory.Shared.Common;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Core.Services;

public sealed class UserService : IUserService
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public UserService(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<IReadOnlyList<UserResponse>>> GetAllAsync(CancellationToken ct = default)
    {
        var users = await _context.Users
            .Include(u => u.Role)
            .OrderBy(u => u.FullName)
            .ToListAsync(ct);

        var response = _mapper.Map<IReadOnlyList<UserResponse>>(users);
        return Result<IReadOnlyList<UserResponse>>.Success(response);
    }

    public async Task<Result<UserResponse>> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var user = await _context.Users
            .Include(u => u.Role)
            .SingleOrDefaultAsync(u => u.Id == id, ct);

        if (user == null)
        {
            return Result<UserResponse>.Failure("User not found.");
        }

        var response = _mapper.Map<UserResponse>(user);
        return Result<UserResponse>.Success(response);
    }

    public async Task<Result<UserResponse>> CreateAsync(UserCreateRequest request, int createdByUserId, CancellationToken ct = default)
    {
        // Unique username validation
        if (await _context.Users.AnyAsync(u => u.UserName == request.UserName, ct))
        {
            return Result<UserResponse>.Failure("Username is already taken.");
        }

        // Validate RoleId exists
        var role = await _context.Roles.SingleOrDefaultAsync(r => r.Id == request.RoleId, ct);
        if (role == null)
        {
            return Result<UserResponse>.Failure("Invalid RoleId.");
        }

        var user = new User
        {
            FullName = request.FullName.Trim(),
            UserName = request.UserName.Trim(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            RoleId = request.RoleId,
            IsActive = request.IsActive,
            CreatedBy = createdByUserId,
            UpdatedBy = createdByUserId
        };

        await _context.Users.AddAsync(user, ct);
        await _context.SaveChangesAsync(ct);

        // Assign the role navigation back for response mapping
        user.Role = role;

        var response = _mapper.Map<UserResponse>(user);
        return Result<UserResponse>.Success(response);
    }

    public async Task<Result<UserResponse>> UpdateAsync(int id, UserUpdateRequest request, int updatedByUserId, CancellationToken ct = default)
    {
        var user = await _context.Users
            .Include(u => u.Role)
            .SingleOrDefaultAsync(u => u.Id == id, ct);

        if (user == null)
        {
            return Result<UserResponse>.Failure("User not found.");
        }

        // Unique username validation if username is changing
        if (user.UserName != request.UserName.Trim() && 
            await _context.Users.AnyAsync(u => u.UserName == request.UserName, ct))
        {
            return Result<UserResponse>.Failure("Username is already taken.");
        }

        // Validate RoleId exists
        var role = await _context.Roles.SingleOrDefaultAsync(r => r.Id == request.RoleId, ct);
        if (role == null)
        {
            return Result<UserResponse>.Failure("Invalid RoleId.");
        }

        user.FullName = request.FullName.Trim();
        user.UserName = request.UserName.Trim();
        user.RoleId = request.RoleId;
        user.IsActive = request.IsActive;
        user.UpdatedBy = updatedByUserId;

        await _context.SaveChangesAsync(ct);

        user.Role = role;

        var response = _mapper.Map<UserResponse>(user);
        return Result<UserResponse>.Success(response);
    }

    public async Task<Result> ChangePasswordAsync(int userId, ChangePasswordRequest request, CancellationToken ct = default)
    {
        var user = await _context.Users.SingleOrDefaultAsync(u => u.Id == userId, ct);
        if (user == null)
        {
            return Result.Failure("User not found.");
        }

        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
        {
            return Result.Failure("Current password is incorrect.");
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        user.UpdatedBy = userId;

        await _context.SaveChangesAsync(ct);
        return Result.Success();
    }

    public async Task<Result> AdminResetPasswordAsync(int targetUserId, AdminResetPasswordRequest request, CancellationToken ct = default)
    {
        var user = await _context.Users.SingleOrDefaultAsync(u => u.Id == targetUserId, ct);
        if (user == null)
        {
            return Result.Failure("User not found.");
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        
        // This is updated by the administrative user, but for simplicity of parameter signature
        // and audit we can leave CreatedBy/UpdatedBy handled appropriately. We don't have acting user ID
        // in signature of this reset method, but we can set UpdatedBy or pass it if needed. Let's just keep the signature.
        
        await _context.SaveChangesAsync(ct);
        return Result.Success();
    }
}
