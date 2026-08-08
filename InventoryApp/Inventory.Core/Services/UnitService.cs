using AutoMapper;
using Inventory.Core.DTOs.Units;
using Inventory.Core.Entities;
using Inventory.Core.Interfaces;
using Inventory.Core.Interfaces.Services;
using Inventory.Shared.Common;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Core.Services;

public sealed class UnitService : IUnitService
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public UnitService(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<IReadOnlyList<UnitResponse>>> GetAllAsync(CancellationToken ct = default)
    {
        var units = await _context.Units
            .OrderBy(u => u.UnitName)
            .ToListAsync(ct);

        var response = _mapper.Map<IReadOnlyList<UnitResponse>>(units);
        return Result<IReadOnlyList<UnitResponse>>.Success(response);
    }

    public async Task<Result<UnitResponse>> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var unit = await _context.Units.SingleOrDefaultAsync(u => u.Id == id, ct);
        if (unit == null)
        {
            return Result<UnitResponse>.Failure("Unit not found.");
        }

        var response = _mapper.Map<UnitResponse>(unit);
        return Result<UnitResponse>.Success(response);
    }

    public async Task<Result<UnitResponse>> CreateAsync(UnitCreateRequest request, int createdByUserId, CancellationToken ct = default)
    {
        var trimmedName = request.UnitName.Trim();
        var trimmedShort = request.ShortName.Trim();

        // Check active duplicates for UnitName
        var nameExists = await _context.Units.AnyAsync(u => 
            u.IsActive && u.UnitName.ToLower() == trimmedName.ToLower(), 
            ct);

        if (nameExists)
        {
            return Result<UnitResponse>.Failure("An active unit with this name already exists.");
        }

        // Check active duplicates for ShortName
        var shortExists = await _context.Units.AnyAsync(u => 
            u.IsActive && u.ShortName.ToLower() == trimmedShort.ToLower(), 
            ct);

        if (shortExists)
        {
            return Result<UnitResponse>.Failure("An active unit with this short name already exists.");
        }

        var unit = new Unit
        {
            UnitName = trimmedName,
            ShortName = trimmedShort,
            IsActive = true,
            CreatedBy = createdByUserId,
            UpdatedBy = createdByUserId
        };

        await _context.Units.AddAsync(unit, ct);
        await _context.SaveChangesAsync(ct);

        var response = _mapper.Map<UnitResponse>(unit);
        return Result<UnitResponse>.Success(response);
    }

    public async Task<Result<UnitResponse>> UpdateAsync(int id, UnitUpdateRequest request, int updatedByUserId, CancellationToken ct = default)
    {
        var unit = await _context.Units.SingleOrDefaultAsync(u => u.Id == id, ct);
        if (unit == null)
        {
            return Result<UnitResponse>.Failure("Unit not found.");
        }

        var trimmedName = request.UnitName.Trim();
        var trimmedShort = request.ShortName.Trim();

        // Exclude current ID from duplicates check
        var isNameChanging = unit.UnitName.ToLower() != trimmedName.ToLower();
        var isDeactivatedToActive = !unit.IsActive && request.IsActive;

        if (isNameChanging || isDeactivatedToActive)
        {
            var nameExists = await _context.Units.AnyAsync(u => 
                u.Id != id && u.IsActive && u.UnitName.ToLower() == trimmedName.ToLower(), 
                ct);

            if (nameExists)
            {
                return Result<UnitResponse>.Failure("An active unit with this name already exists.");
            }
        }

        var isShortChanging = unit.ShortName.ToLower() != trimmedShort.ToLower();
        if (isShortChanging || isDeactivatedToActive)
        {
            var shortExists = await _context.Units.AnyAsync(u => 
                u.Id != id && u.IsActive && u.ShortName.ToLower() == trimmedShort.ToLower(), 
                ct);

            if (shortExists)
            {
                return Result<UnitResponse>.Failure("An active unit with this short name already exists.");
            }
        }

        unit.UnitName = trimmedName;
        unit.ShortName = trimmedShort;
        unit.IsActive = request.IsActive;
        unit.UpdatedBy = updatedByUserId;

        await _context.SaveChangesAsync(ct);

        var response = _mapper.Map<UnitResponse>(unit);
        return Result<UnitResponse>.Success(response);
    }

    public async Task<Result> SoftDeleteAsync(int id, int deletedByUserId, CancellationToken ct = default)
    {
        var unit = await _context.Units.SingleOrDefaultAsync(u => u.Id == id, ct);
        if (unit == null)
        {
            return Result.Failure("Unit not found.");
        }

        unit.IsActive = false;
        unit.UpdatedBy = deletedByUserId;

        await _context.SaveChangesAsync(ct);
        return Result.Success();
    }
}
