using AutoMapper;
using ECommerce.Application.DTOs;
using ECommerce.Application.Exceptions;
using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Services;

public class ModelService : IModelService
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;
    private readonly ITenantService _tenantService;

    public ModelService(AppDbContext context, IMapper mapper, ITenantService tenantService)
    {
        _context = context;
        _mapper = mapper;
        _tenantService = tenantService;
    }

    public async Task<IReadOnlyList<ModelDto>> GetAllAsync()
    {
        var currentCompanyId = _tenantService.GetCompanyId();
        var isSuperAdmin = _tenantService.IsSuperAdmin();

        IQueryable<Model> query = _context.Models
            .Include(m => m.Brand)
            .AsNoTracking();

        if (!isSuperAdmin && currentCompanyId.HasValue)
        {
            query = query.Where(m => m.CompanyId == currentCompanyId.Value);
        }

        var models = await query
            .OrderBy(m => m.BrandId)
            .ThenBy(m => m.Name)
            .ToListAsync();

        return _mapper.Map<IReadOnlyList<ModelDto>>(models);
    }

    public async Task<IReadOnlyList<ModelDto>> GetByBrandIdAsync(int brandId)
    {
        // Brand'ın company'sini al, böylece doğru tenant context'te model'leri sorguayz
        var brand = await _context.Brands.AsNoTracking().FirstOrDefaultAsync(b => b.Id == brandId);
        if (brand == null)
        {
            return new List<ModelDto>();
        }
        IQueryable<Model> query = _context.Models
            .Include(m => m.Brand)
            .Where(m => m.BrandId == brandId && m.CompanyId == brand.CompanyId)
            .AsNoTracking();
        var models = await query
            .OrderBy(m => m.Name)
            .ToListAsync();
        if (models.Count > 0)
        {
            foreach (var m in models)
            {
            }
        }

        var result = _mapper.Map<IReadOnlyList<ModelDto>>(models);
        return result;
    }

    public async Task<ModelDto?> GetByIdAsync(int id)
    {
        var model = await _context.Models
            .Include(m => m.Brand)
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id);

        return model == null ? null : _mapper.Map<ModelDto>(model);
    }

    public async Task<ModelDto> CreateAsync(ModelFormDto dto)
    {
        var currentCompanyId = _tenantService.GetCompanyId();
        var isSuperAdmin = _tenantService.IsSuperAdmin();
        var companyId = currentCompanyId ?? (isSuperAdmin ? 1 : throw new BusinessException("Company context is required"));
        // Verify brand exists
        var brand = await _context.Brands.FindAsync(dto.BrandId);
        if (brand == null)
        {
            throw new KeyNotFoundException($"Brand with ID {dto.BrandId} not found");
        }
        var model = Model.Create(dto.Name, dto.Description ?? string.Empty, dto.BrandId, companyId);

        if (!dto.IsActive)
        {
            model.Deactivate();
        }

        _context.Models.Add(model);
        await _context.SaveChangesAsync();
        var result = _mapper.Map<ModelDto>(model);
        return result;
    }

    public async Task UpdateAsync(ModelFormDto dto)
    {
        if (!dto.Id.HasValue) throw new Exception("Model id is required");
        var model = await _context.Models.FirstOrDefaultAsync(m => m.Id == dto.Id.Value);

        if (model == null)
        {
            throw new KeyNotFoundException($"Model with ID {dto.Id} not found");
        }

        model.Update(dto.Name, dto.Description ?? string.Empty);

        if (dto.BrandId != model.BrandId)
        {
            model.ChangeBrand(dto.BrandId);
        }

        if (dto.IsActive)
        {
            model.Activate();
        }
        else
        {
            model.Deactivate();
        }

        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var model = await _context.Models
            .Include(m => m.Products)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (model == null)
        {
            throw new KeyNotFoundException($"Model with ID {id} not found");
        }
        if (model.Products.Any())
        {
            throw new BusinessException("Bu model silinemez çünkü ürünlerde kullanılıyor.");
        }

        _context.Models.Remove(model);
        await _context.SaveChangesAsync();
    }
}
