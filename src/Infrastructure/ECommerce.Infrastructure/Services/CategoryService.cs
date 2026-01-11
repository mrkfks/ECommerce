using AutoMapper;
using ECommerce.Application.DTOs;
using ECommerce.Application.Exceptions;
using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ECommerce.Infrastructure.Services;

public class CategoryService : ICategoryService
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;
    private readonly ITenantService _tenantService;
    private readonly ILogger<CategoryService> _logger;

    public CategoryService(AppDbContext context, IMapper mapper, ITenantService tenantService, ILogger<CategoryService> logger)
    {
        _context = context;
        _mapper = mapper;
        _tenantService = tenantService;
        _logger = logger;
    }

    public async Task<IReadOnlyList<CategoryDto>> GetAllAsync()
    {
        var currentCompanyId = _tenantService.GetCompanyId();
        _logger.LogInformation("CategoryService.GetAllAsync - CompanyId from TenantService: {CompanyId}", currentCompanyId);

        // EF Query Filter'ları bypass et, manuel filtre uygulayacağız
        IQueryable<Category> query = _context.Categories.IgnoreQueryFilters();

        // Ürün sorgusu: soft delete ve şirket filtresi
        IQueryable<Product> productsQuery = _context.Products
            .IgnoreQueryFilters()
            .Where(p => !p.IsDeleted);

        // IsDeleted kontrolü
        query = query.Where(c => !c.IsDeleted);

        // Company ID varsa filtrele (SuperAdmin dahil)
        if (currentCompanyId.HasValue)
        {
            _logger.LogInformation("Filtering categories by CompanyId: {CompanyId}", currentCompanyId.Value);
            query = query.Where(c => c.CompanyId == currentCompanyId.Value);
            productsQuery = productsQuery.Where(p => p.CompanyId == currentCompanyId.Value);
        }
        else
        {
            _logger.LogWarning("No CompanyId filter applied - will return all categories");
        }

        var categoriesWithCounts = await query
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.Name)
            .Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                ImageUrl = c.ImageUrl,
                IsActive = c.IsActive,
                ProductCount = productsQuery.Count(p => p.CategoryId == c.Id),
                ParentCategoryId = c.ParentCategoryId,
                DisplayOrder = c.DisplayOrder,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt
            })
            .ToListAsync();

        _logger.LogInformation("Found {Count} categories", categoriesWithCounts.Count);
        return categoriesWithCounts;
    }

    public async Task<CategoryDto?> GetByIdAsync(int id)
    {
        var category = await _context.Categories.FindAsync(id);

        if (category == null) return null;

        return _mapper.Map<CategoryDto>(category);
    }

    public async Task<CategoryDto> CreateAsync(CategoryCreateDto dto)
    {
        var currentCompanyId = _tenantService.GetCompanyId();
        var isSuperAdmin = _tenantService.IsSuperAdmin();

        // SuperAdmin için CompanyId yoksa varsayılan olarak 1 kullan (sistem şirketi)
        var companyId = currentCompanyId ?? (isSuperAdmin ? 1 : throw new BusinessException("Company context is required"));

        var category = Category.Create(
            dto.Name,
            dto.Description ?? string.Empty,
            dto.ImageUrl,
            companyId,
            dto.ParentCategoryId,
            dto.DisplayOrder
        );

        if (dto.IsActive)
        {
            category.Activate();
        }
        else
        {
            category.Deactivate();
        }

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        return _mapper.Map<CategoryDto>(category);
    }

    public async Task UpdateAsync(CategoryUpdateDto dto)
    {
        var category = await _context.Categories.FindAsync(dto.Id);

        if (category == null)
        {
            throw new KeyNotFoundException($"Category with ID {dto.Id} not found");
        }

        category.Update(dto.Name, dto.Description ?? string.Empty, dto.ImageUrl, dto.ParentCategoryId, dto.DisplayOrder);

        if (dto.IsActive)
        {
            category.Activate();
        }
        else
        {
            category.Deactivate();
        }

        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var category = await _context.Categories.FindAsync(id);

        if (category == null)
        {
            throw new KeyNotFoundException($"Category with ID {id} not found");
        }

        // Check if category has subcategories
        var hasSubcategories = await _context.Categories
            .AnyAsync(c => c.ParentCategoryId == id);

        if (hasSubcategories)
        {
            throw new BusinessException("Alt kategorileri olan kategori silinemez");
        }

        // Check if category has products
        var hasProducts = await _context.Products
            .AnyAsync(p => p.CategoryId == id);

        if (hasProducts)
        {
            throw new BusinessException("Ürünleri olan kategori silinemez");
        }

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();
    }
}
