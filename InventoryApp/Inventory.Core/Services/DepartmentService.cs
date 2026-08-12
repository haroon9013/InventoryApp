using AutoMapper;
using Inventory.Core.DTOs.Departments;
using Inventory.Core.Entities;
using Inventory.Core.Interfaces;
using Inventory.Core.Interfaces.Services;
using Inventory.Shared.Common;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Core.Services;

public sealed class DepartmentService : IDepartmentService
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public DepartmentService(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<IReadOnlyList<DepartmentResponse>>> GetAllAsync(bool includeInactive = false, CancellationToken ct = default)
    {
        var query = _context.Departments.AsQueryable();

        if (!includeInactive)
        {
            query = query.Where(d => d.IsActive);
        }

        var departments = await query
            .OrderBy(d => d.DepartmentName)
            .ToListAsync(ct);

        var response = _mapper.Map<IReadOnlyList<DepartmentResponse>>(departments);
        return Result<IReadOnlyList<DepartmentResponse>>.Success(response);
    }

    public async Task<Result<DepartmentResponse>> GetByIdAsync(int id, bool includeInactive = false, CancellationToken ct = default)
    {
        var department = await _context.Departments.SingleOrDefaultAsync(d => d.Id == id, ct);
        if (department == null || (!includeInactive && !department.IsActive))
        {
            return Result<DepartmentResponse>.Failure("Department not found.");
        }

        var response = _mapper.Map<DepartmentResponse>(department);
        return Result<DepartmentResponse>.Success(response);
    }

    public async Task<Result<DepartmentResponse>> CreateAsync(DepartmentCreateRequest request, int createdByUserId, CancellationToken ct = default)
    {
        var trimmedName = request.DepartmentName.Trim();

        // Check active duplicates for DepartmentName (case-insensitive)
        var exists = await _context.Departments.AnyAsync(d => 
            d.IsActive && d.DepartmentName.ToLower() == trimmedName.ToLower(), 
            ct);

        if (exists)
        {
            return Result<DepartmentResponse>.Failure("An active department with this name already exists.");
        }

        var department = new Department
        {
            DepartmentName = trimmedName,
            Description = request.Description?.Trim(),
            IsActive = true,
            CreatedBy = createdByUserId,
            UpdatedBy = createdByUserId
        };

        await _context.Departments.AddAsync(department, ct);
        await _context.SaveChangesAsync(ct);

        var response = _mapper.Map<DepartmentResponse>(department);
        return Result<DepartmentResponse>.Success(response);
    }

    public async Task<Result<DepartmentResponse>> UpdateAsync(int id, DepartmentUpdateRequest request, int updatedByUserId, CancellationToken ct = default)
    {
        var department = await _context.Departments.SingleOrDefaultAsync(d => d.Id == id, ct);
        if (department == null)
        {
            return Result<DepartmentResponse>.Failure("Department not found.");
        }

        var trimmedName = request.DepartmentName.Trim();

        // Exclude current ID from duplicates check
        var isNameChanging = department.DepartmentName.ToLower() != trimmedName.ToLower();
        var isDeactivatedToActive = !department.IsActive && request.IsActive;

        if (isNameChanging || isDeactivatedToActive)
        {
            var exists = await _context.Departments.AnyAsync(d => 
                d.Id != id && d.IsActive && d.DepartmentName.ToLower() == trimmedName.ToLower(), 
                ct);

            if (exists)
            {
                return Result<DepartmentResponse>.Failure("An active department with this name already exists.");
            }
        }

        department.DepartmentName = trimmedName;
        department.Description = request.Description?.Trim();
        department.IsActive = request.IsActive;
        department.UpdatedBy = updatedByUserId;

        await _context.SaveChangesAsync(ct);

        var response = _mapper.Map<DepartmentResponse>(department);
        return Result<DepartmentResponse>.Success(response);
    }

    public async Task<Result> SoftDeleteAsync(int id, int deletedByUserId, CancellationToken ct = default)
    {
        var department = await _context.Departments.SingleOrDefaultAsync(d => d.Id == id, ct);
        if (department == null)
        {
            return Result.Failure("Department not found.");
        }

        department.IsActive = false;
        department.UpdatedBy = deletedByUserId;

        await _context.SaveChangesAsync(ct);
        return Result.Success();
    }
}
