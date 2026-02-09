using AutoMapper;
using ECommerce.Application.DTOs;
using ECommerce.Application.Exceptions;
using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Services;

public class BrandService : IBrandService
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;
    private readonly ITenantService _tenantService;

    public BrandService(AppDbContext context, IMapper mapper, ITenantService tenantService)
    {
        _context = context;
        _mapper = mapper;
        _tenantService = tenantService;
    }

    public async Task<IReadOnlyList<BrandDto>> GetAllAsync()
    {
        var currentCompanyId = _tenantService.GetCompanyId();
        var isSuperAdmin = _tenantService.IsSuperAdmin();

        IQueryable<Brand> query = _context.Brands.AsNoTracking();

        if (!isSuperAdmin && currentCompanyId.HasValue)
        {
            query = query.Where(b => b.CompanyId == currentCompanyId.Value);
        }

        var brands = await query
            .OrderBy(b => b.Name)
            .ToListAsync();

        return _mapper.Map<IReadOnlyList<BrandDto>>(brands);
    }

    public async Task<BrandDto?> GetByIdAsync(int id)
    {
        var brand = await _context.Brands.AsNoTracking().FirstOrDefaultAsync(b => b.Id == id);
        return brand == null ? null : _mapper.Map<BrandDto>(brand);
    }

    public async Task<BrandDto> CreateAsync(BrandFormDto dto)
    {
        var currentCompanyId = _tenantService.GetCompanyId();
        var isSuperAdmin = _tenantService.IsSuperAdmin();

        var companyId = currentCompanyId ?? (isSuperAdmin ? 1 : throw new BusinessException("Company context is required"));

        var brand = Brand.Create(dto.Name, dto.Description ?? string.Empty, companyId, dto.ImageUrl, dto.IsActive);

        _context.Brands.Add(brand);
        await _context.SaveChangesAsync();

        return _mapper.Map<BrandDto>(brand);
    }

    public async Task UpdateAsync(BrandFormDto dto)
    {
        if (!dto.Id.HasValue) throw new BusinessException("Brand ID is required for update.");
        var brand = await _context.Brands.FirstOrDefaultAsync(b => b.Id == dto.Id.Value);

        if (brand == null)
        {
            throw new KeyNotFoundException($"Brand with ID {dto.Id} not found");
        }

        brand.Update(dto.Name, dto.Description ?? string.Empty, dto.ImageUrl);

        if (dto.IsActive)
        {
            brand.Activate();
        }
        else
        {
            brand.Deactivate();
        }

        await _context.SaveChangesAsync();
    }

    public async Task UpdateImageAsync(int id, string imageUrl)
    {
        var brand = await _context.Brands.FirstOrDefaultAsync(b => b.Id == id);
        if (brand == null) throw new KeyNotFoundException($"Brand with ID {id} not found");

        brand.Update(brand.Name, brand.Description, imageUrl);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var brand = await _context.Brands
            .Include(b => b.Products)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (brand == null)
        {
            throw new KeyNotFoundException($"Brand with ID {id} not found");
        }

        if (brand.Products.Any())
        {
            throw new BusinessException("Ürünleri olan marka silinemez");
        }

        _context.Brands.Remove(brand);
        await _context.SaveChangesAsync();
    }
}
