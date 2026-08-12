using AutoMapper;
using Inventory.Core.DTOs.Products;
using Inventory.Core.Entities;
using Inventory.Core.Interfaces;
using Inventory.Core.Interfaces.Services;
using Inventory.Shared.Common;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Core.Services;

public sealed class ProductService : IProductService
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public ProductService(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<IReadOnlyList<ProductResponse>>> GetAllAsync(bool includeInactive = false, CancellationToken ct = default)
    {
        var query = _context.Products
            .Include(p => p.Category)
            .Include(p => p.Unit)
            .AsQueryable();

        if (!includeInactive)
        {
            query = query.Where(p => p.IsActive);
        }

        var products = await query
            .OrderBy(p => p.ProductName)
            .ToListAsync(ct);

        var response = _mapper.Map<IReadOnlyList<ProductResponse>>(products);
        return Result<IReadOnlyList<ProductResponse>>.Success(response);
    }

    public async Task<Result<ProductResponse>> GetByIdAsync(int id, bool includeInactive = false, CancellationToken ct = default)
    {
        var query = _context.Products
            .Include(p => p.Category)
            .Include(p => p.Unit)
            .AsQueryable();

        var product = await query.SingleOrDefaultAsync(p => p.Id == id, ct);
        if (product == null || (!includeInactive && !product.IsActive))
        {
            return Result<ProductResponse>.Failure("Product not found.");
        }

        var response = _mapper.Map<ProductResponse>(product);
        return Result<ProductResponse>.Success(response);
    }

    public async Task<Result<ProductResponse>> CreateAsync(ProductCreateRequest request, int createdByUserId, CancellationToken ct = default)
    {
        var trimmedCode = request.ProductCode.Trim().ToUpper();
        var trimmedName = request.ProductName.Trim();

        // 1. Check ProductCode uniqueness (case-insensitive across ALL products in DB)
        var codeExists = await _context.Products.AnyAsync(p => 
            p.ProductCode.ToLower() == trimmedCode.ToLower(), 
            ct);

        if (codeExists)
        {
            return Result<ProductResponse>.Failure("A product with this product code already exists.");
        }

        // 2. Validate CategoryId points to active category
        var category = await _context.Categories.SingleOrDefaultAsync(c => c.Id == request.CategoryId, ct);
        if (category == null || !category.IsActive)
        {
            return Result<ProductResponse>.Failure("Category not found or is inactive.");
        }

        // 3. Validate UnitId points to active unit
        var unit = await _context.Units.SingleOrDefaultAsync(u => u.Id == request.UnitId, ct);
        if (unit == null || !unit.IsActive)
        {
            return Result<ProductResponse>.Failure("Unit not found or is inactive.");
        }

        var product = new Product
        {
            ProductCode = trimmedCode,
            ProductName = trimmedName,
            CategoryId = request.CategoryId,
            UnitId = request.UnitId,
            MinimumStock = request.MinimumStock,
            CurrentStock = 0m,
            LastPurchasePrice = 0m,
            IsActive = true,
            CreatedBy = createdByUserId,
            UpdatedBy = createdByUserId
        };

        await _context.Products.AddAsync(product, ct);
        await _context.SaveChangesAsync(ct);

        // Fetch again to ensure navigation properties are populated for Response mapping
        var savedProduct = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Unit)
            .SingleAsync(p => p.Id == product.Id, ct);

        var response = _mapper.Map<ProductResponse>(savedProduct);
        return Result<ProductResponse>.Success(response);
    }

    public async Task<Result<ProductResponse>> UpdateAsync(int id, ProductUpdateRequest request, int updatedByUserId, CancellationToken ct = default)
    {
        var product = await _context.Products.SingleOrDefaultAsync(p => p.Id == id, ct);
        if (product == null)
        {
            return Result<ProductResponse>.Failure("Product not found.");
        }

        var trimmedCode = request.ProductCode.Trim().ToUpper();
        var trimmedName = request.ProductName.Trim();

        // 1. Check ProductCode uniqueness (case-insensitive across ALL products in DB, excluding self)
        var codeExists = await _context.Products.AnyAsync(p => 
            p.Id != id && p.ProductCode.ToLower() == trimmedCode.ToLower(), 
            ct);

        if (codeExists)
        {
            return Result<ProductResponse>.Failure("A product with this product code already exists.");
        }

        // 2. Validate CategoryId points to active category
        var category = await _context.Categories.SingleOrDefaultAsync(c => c.Id == request.CategoryId, ct);
        if (category == null || !category.IsActive)
        {
            return Result<ProductResponse>.Failure("Category not found or is inactive.");
        }

        // 3. Validate UnitId points to active unit
        var unit = await _context.Units.SingleOrDefaultAsync(u => u.Id == request.UnitId, ct);
        if (unit == null || !unit.IsActive)
        {
            return Result<ProductResponse>.Failure("Unit not found or is inactive.");
        }

        product.ProductCode = trimmedCode;
        product.ProductName = trimmedName;
        product.CategoryId = request.CategoryId;
        product.UnitId = request.UnitId;
        product.MinimumStock = request.MinimumStock;
        product.IsActive = request.IsActive;
        product.UpdatedBy = updatedByUserId;

        await _context.SaveChangesAsync(ct);

        // Fetch again to ensure navigation properties are populated for Response mapping
        var savedProduct = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Unit)
            .SingleAsync(p => p.Id == product.Id, ct);

        var response = _mapper.Map<ProductResponse>(savedProduct);
        return Result<ProductResponse>.Success(response);
    }

    public async Task<Result> SoftDeleteAsync(int id, int deletedByUserId, CancellationToken ct = default)
    {
        var product = await _context.Products.SingleOrDefaultAsync(p => p.Id == id, ct);
        if (product == null)
        {
            return Result.Failure("Product not found.");
        }

        product.IsActive = false;
        product.UpdatedBy = deletedByUserId;

        await _context.SaveChangesAsync(ct);
        return Result.Success();
    }
}
