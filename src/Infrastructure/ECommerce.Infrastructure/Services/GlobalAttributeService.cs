using AutoMapper;
using ECommerce.Application.DTOs;
using ECommerce.Application.Exceptions;
using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Services;

public class GlobalAttributeService : IGlobalAttributeService
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;
    private readonly ITenantService _tenantService;

    public GlobalAttributeService(AppDbContext context, IMapper mapper, ITenantService tenantService)
    {
        _context = context;
        _mapper = mapper;
        _tenantService = tenantService;
    }

    public async Task<IReadOnlyList<GlobalAttributeDto>> GetAllAsync()
    {
        var currentCompanyId = _tenantService.GetCompanyId();
        var isSuperAdmin = _tenantService.IsSuperAdmin();

        IQueryable<GlobalAttribute> query = _context.GlobalAttributes
            .Include(g => g.Values)
            .AsNoTracking();

        if (!isSuperAdmin && currentCompanyId.HasValue)
        {
            query = query.Where(g => g.CompanyId == currentCompanyId.Value);
        }

        var list = await query
            .OrderBy(g => g.DisplayOrder)
            .ThenBy(g => g.Name)
            .ToListAsync();

        return _mapper.Map<IReadOnlyList<GlobalAttributeDto>>(list);
    }

    public async Task<GlobalAttributeDto?> GetByIdAsync(int id)
    {
        var entity = await _context.GlobalAttributes
            .Include(g => g.Values)
            .AsNoTracking()
            .FirstOrDefaultAsync(g => g.Id == id);

        return entity == null ? null : _mapper.Map<GlobalAttributeDto>(entity);
    }

    public async Task<GlobalAttributeDto> CreateAsync(GlobalAttributeCreateDto dto)
    {
        var currentCompanyId = _tenantService.GetCompanyId();
        var isSuperAdmin = _tenantService.IsSuperAdmin();

        var companyId = currentCompanyId ?? (isSuperAdmin ? 1 : throw new BusinessException("Company context is required"));

        var entity = GlobalAttribute.Create(
            dto.Name,
            dto.DisplayName,
            dto.Description ?? string.Empty,
            companyId,
            (AttributeType)dto.AttributeType,
            dto.DisplayOrder);

        if (!dto.IsActive)
            entity.Deactivate();

        _context.GlobalAttributes.Add(entity);
        await _context.SaveChangesAsync();

        if (dto.Values.Any())
        {
            foreach (var valueDto in dto.Values)
            {
                var value = GlobalAttributeValue.Create(entity.Id, valueDto.Value, valueDto.ColorCode, valueDto.DisplayOrder);
                if (!valueDto.IsActive)
                    value.Deactivate();
                _context.GlobalAttributeValues.Add(value);
            }
            await _context.SaveChangesAsync();
        }

        var reloaded = await _context.GlobalAttributes
            .Include(g => g.Values)
            .AsNoTracking()
            .FirstAsync(g => g.Id == entity.Id);

        return _mapper.Map<GlobalAttributeDto>(reloaded);
    }

    public async Task UpdateAsync(GlobalAttributeUpdateDto dto)
    {
        var entity = await _context.GlobalAttributes
            .Include(g => g.Values)
            .FirstOrDefaultAsync(g => g.Id == dto.Id);

        if (entity == null)
            throw new KeyNotFoundException($"Global attribute with ID {dto.Id} not found");

        entity.Update(dto.DisplayName, dto.Description ?? string.Empty, (AttributeType)dto.AttributeType, dto.DisplayOrder);
        if (dto.IsActive) entity.Activate(); else entity.Deactivate();

        // replace values for simplicity
        var existingValues = entity.Values.ToList();
        if (existingValues.Any())
        {
            _context.GlobalAttributeValues.RemoveRange(existingValues);
        }

        foreach (var valueDto in dto.Values)
        {
            var value = GlobalAttributeValue.Create(entity.Id, valueDto.Value, valueDto.ColorCode, valueDto.DisplayOrder);
            if (!valueDto.IsActive)
                value.Deactivate();
            _context.GlobalAttributeValues.Add(value);
        }

        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _context.GlobalAttributes
            .Include(g => g.Values)
            .FirstOrDefaultAsync(g => g.Id == id);

        if (entity == null)
            return;

        if (entity.Values.Any())
        {
            _context.GlobalAttributeValues.RemoveRange(entity.Values);
        }

        _context.GlobalAttributes.Remove(entity);
        await _context.SaveChangesAsync();
    }
}
