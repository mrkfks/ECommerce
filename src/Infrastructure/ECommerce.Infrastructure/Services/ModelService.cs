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
        Console.WriteLine($"\n[ModelService.GetByBrandIdAsync] Başladı - BrandId: {brandId}");
        
        // Brand'ın company'sini al, böylece doğru tenant context'te model'leri sorguayz
        var brand = await _context.Brands.AsNoTracking().FirstOrDefaultAsync(b => b.Id == brandId);
        if (brand == null)
        {
            Console.WriteLine($"[ModelService.GetByBrandIdAsync] Brand {brandId} bulunamadı!");
            return new List<ModelDto>();
        }

        Console.WriteLine($"[ModelService.GetByBrandIdAsync] Brand bulundu - CompanyId: {brand.CompanyId}");

        IQueryable<Model> query = _context.Models
            .Include(m => m.Brand)
            .Where(m => m.BrandId == brandId && m.CompanyId == brand.CompanyId)
            .AsNoTracking();

        Console.WriteLine($"[ModelService.GetByBrandIdAsync] Query oluşturuldu");

        var models = await query
            .OrderBy(m => m.Name)
            .ToListAsync();

        Console.WriteLine($"[ModelService.GetByBrandIdAsync] Sorgu çalıştırıldı - Bulunan model sayısı: {models.Count}");
        
        if (models.Count > 0)
        {
            foreach (var m in models)
            {
                Console.WriteLine($"  - Model: Id={m.Id}, Name={m.Name}, BrandId={m.BrandId}, CompanyId={m.CompanyId}");
            }
        }

        var result = _mapper.Map<IReadOnlyList<ModelDto>>(models);
        Console.WriteLine($"[ModelService.GetByBrandIdAsync] Mapping tamamlandı - DTO sayısı: {result.Count}\n");
        
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

    public async Task<ModelDto> CreateAsync(ModelCreateDto dto)
    {
        Console.WriteLine($"[ModelService.CreateAsync] Başladı - Name: {dto.Name}, BrandId: {dto.BrandId}");
        
        var currentCompanyId = _tenantService.GetCompanyId();
        var isSuperAdmin = _tenantService.IsSuperAdmin();

        Console.WriteLine($"[ModelService.CreateAsync] CompanyId: {currentCompanyId}, IsSuperAdmin: {isSuperAdmin}");

        var companyId = currentCompanyId ?? (isSuperAdmin ? 1 : throw new BusinessException("Company context is required"));

        Console.WriteLine($"[ModelService.CreateAsync] Kullanılacak CompanyId: {companyId}");

        // Verify brand exists
        var brand = await _context.Brands.FindAsync(dto.BrandId);
        if (brand == null)
        {
            Console.WriteLine($"[ModelService.CreateAsync] Brand {dto.BrandId} bulunamadı!");
            throw new KeyNotFoundException($"Brand with ID {dto.BrandId} not found");
        }

        Console.WriteLine($"[ModelService.CreateAsync] Brand bulundu - CompanyId: {brand.CompanyId}");

        var model = Model.Create(dto.Name, dto.Description ?? string.Empty, dto.BrandId, companyId);

        if (!dto.IsActive)
        {
            model.Deactivate();
        }

        _context.Models.Add(model);
        Console.WriteLine($"[ModelService.CreateAsync] Model context'e eklendi");
        
        await _context.SaveChangesAsync();
        Console.WriteLine($"[ModelService.CreateAsync] SaveChangesAsync tamamlandı - Model.Id: {model.Id}");

        var result = _mapper.Map<ModelDto>(model);
        Console.WriteLine($"[ModelService.CreateAsync] Mapping tamamlandı - ModelDto.Id: {result.Id}");
        
        return result;
    }

    public async Task UpdateAsync(ModelUpdateDto dto)
    {
        var model = await _context.Models.FirstOrDefaultAsync(m => m.Id == dto.Id);

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
        Console.WriteLine($"[ModelService.DeleteAsync] Başladı - Id: {id}");
        
        var model = await _context.Models
            .Include(m => m.Products)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (model == null)
        {
            Console.WriteLine($"[ModelService.DeleteAsync] Model {id} bulunamadı!");
            throw new KeyNotFoundException($"Model with ID {id} not found");
        }

        Console.WriteLine($"[ModelService.DeleteAsync] Model bulundu - Name: {model.Name}");

        if (model.Products.Any())
        {
            Console.WriteLine($"[ModelService.DeleteAsync] Model ürünlerde kullanılıyor!");
            throw new BusinessException("Bu model silinemez çünkü ürünlerde kullanılıyor.");
        }

        _context.Models.Remove(model);
        Console.WriteLine($"[ModelService.DeleteAsync] Model context'den kaldırıldı");
        
        await _context.SaveChangesAsync();
        Console.WriteLine($"[ModelService.DeleteAsync] SaveChangesAsync tamamlandı\n");
    }
}
