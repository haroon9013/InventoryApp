using AutoMapper;
using Inventory.Core.DTOs.Categories;
using Inventory.Core.Entities;
using Inventory.Core.Interfaces;
using Inventory.Core.Interfaces.Services;
using Inventory.Shared.Common;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Core.Services;

public sealed class CategoryService : ICategoryService
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public CategoryService(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<IReadOnlyList<CategoryResponse>>> GetAllAsync(bool includeInactive = false, CancellationToken ct = default)
    {
        var query = _context.Categories.AsQueryable();

        if (!includeInactive)
        {
            query = query.Where(c => c.IsActive);
        }

        var categories = await query
            .OrderBy(c => c.CategoryName)
            .ToListAsync(ct);

        var response = _mapper.Map<IReadOnlyList<CategoryResponse>>(categories);
        return Result<IReadOnlyList<CategoryResponse>>.Success(response);
    }

    public async Task<Result<CategoryResponse>> GetByIdAsync(int id, bool includeInactive = false, CancellationToken ct = default)
    {
        var category = await _context.Categories.SingleOrDefaultAsync(c => c.Id == id, ct);
        if (category == null || (!includeInactive && !category.IsActive))
        {
            return Result<CategoryResponse>.Failure("Category not found.");
        }

        var response = _mapper.Map<CategoryResponse>(category);
        return Result<CategoryResponse>.Success(response);
    }

    public async Task<Result<CategoryResponse>> CreateAsync(CategoryCreateRequest request, int createdByUserId, CancellationToken ct = default)
    {
        var trimmedName = request.CategoryName.Trim();

        // Check duplicate active category names (case-insensitive)
        var exists = await _context.Categories.AnyAsync(c => 
            c.IsActive && c.CategoryName.ToLower() == trimmedName.ToLower(), 
            ct);

        if (exists)
        {
            return Result<CategoryResponse>.Failure("An active category with this name already exists.");
        }

        var category = new Category
        {
            CategoryName = trimmedName,
            Description = request.Description?.Trim(),
            IsActive = true,
            CreatedBy = createdByUserId,
            UpdatedBy = createdByUserId
        };

        await _context.Categories.AddAsync(category, ct);
        await _context.SaveChangesAsync(ct);

        var response = _mapper.Map<CategoryResponse>(category);
        return Result<CategoryResponse>.Success(response);
    }

    public async Task<Result<CategoryResponse>> UpdateAsync(int id, CategoryUpdateRequest request, int updatedByUserId, CancellationToken ct = default)
    {
        var category = await _context.Categories.SingleOrDefaultAsync(c => c.Id == id, ct);
        if (category == null)
        {
            return Result<CategoryResponse>.Failure("Category not found.");
        }

        var trimmedName = request.CategoryName.Trim();

        // Only check duplicates if the name is changing or we are changing IsActive to true
        var isNameChanging = category.CategoryName.ToLower() != trimmedName.ToLower();
        var isDeactivatedToActive = !category.IsActive && request.IsActive;

        if (isNameChanging || isDeactivatedToActive)
        {
            var exists = await _context.Categories.AnyAsync(c => 
                c.Id != id && c.IsActive && c.CategoryName.ToLower() == trimmedName.ToLower(), 
                ct);

            if (exists)
            {
                return Result<CategoryResponse>.Failure("An active category with this name already exists.");
            }
        }

        category.CategoryName = trimmedName;
        category.Description = request.Description?.Trim();
        category.IsActive = request.IsActive;
        category.UpdatedBy = updatedByUserId;

        await _context.SaveChangesAsync(ct);

        var response = _mapper.Map<CategoryResponse>(category);
        return Result<CategoryResponse>.Success(response);
    }

    public async Task<Result> SoftDeleteAsync(int id, int deletedByUserId, CancellationToken ct = default)
    {
        var category = await _context.Categories.SingleOrDefaultAsync(c => c.Id == id, ct);
        if (category == null)
        {
            return Result.Failure("Category not found.");
        }

        // soft delete: set IsActive = false
        category.IsActive = false;
        category.UpdatedBy = deletedByUserId;

        await _context.SaveChangesAsync(ct);
        return Result.Success();
    }
}
