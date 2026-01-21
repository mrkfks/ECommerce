using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ECommerce.Infrastructure.Services;

public class CampaignService : ICampaignService
{
    private readonly AppDbContext _context;
    private readonly ITenantService _tenantService;
    private readonly ILogger<CampaignService> _logger;

    public CampaignService(AppDbContext context, ITenantService tenantService, ILogger<CampaignService> logger)
    {
        _context = context;
        _tenantService = tenantService;
        _logger = logger;
    }

    private async Task<Campaign?> GetCampaignEntityAsync(int id)
    {
         var campaign = await _context.Campaigns.FindAsync(id);
         if (campaign == null) return null;
         
         // Tenant check if needed
         var companyId = _tenantService.GetCompanyId();
         if (companyId.HasValue && campaign.CompanyId != companyId.Value)
         {
             // return null or throw? usually null or unauthorized.
             // For now return null acts as not found/accessible.
             return null;
         }
         return campaign;
    }

    public async Task<IReadOnlyList<CampaignDto>> GetAllAsync()
    {
        var query = _context.Campaigns
            .Include(c => c.Company)
            .AsNoTracking();

        var companyId = _tenantService.GetCompanyId();
        if (companyId.HasValue) query = query.Where(c => c.CompanyId == companyId.Value);

        var campaigns = await query
            .OrderByDescending(c => c.IsActive) // Logic from controller was diff?
            .ThenByDescending(c => c.CreatedAt)
            .ToListAsync();
            
        // Controller sort: OrderByDescending(c => c.IsCurrentlyActive).ThenByDescending(c => c.CreatedAt)
        // I'll map first then sort. Or sort in memory.
        
        return campaigns.Select(MapToDto).OrderByDescending(c => c.IsCurrentlyActive).ThenByDescending(c => c.CreatedAt).ToList();
    }

    public async Task<IReadOnlyList<CampaignDto>> GetActiveAsync()
    {
        var now = DateTime.UtcNow;
        var query = _context.Campaigns
            .Include(c => c.Company)
            .Where(c => c.IsActive && now >= c.StartDate && now <= c.EndDate)
            .AsNoTracking();

        var companyId = _tenantService.GetCompanyId();
        if (companyId.HasValue) query = query.Where(c => c.CompanyId == companyId.Value);

        var campaigns = await query
            .OrderBy(c => c.EndDate)
            .ToListAsync();

        return campaigns.Select(MapToDto).ToList();
    }

    public async Task<CampaignSummaryDto> GetSummaryAsync()
    {
        var now = DateTime.UtcNow;
        var query = _context.Campaigns.Include(c => c.Company).AsNoTracking();
        
        var companyId = _tenantService.GetCompanyId();
        if (companyId.HasValue) query = query.Where(c => c.CompanyId == companyId.Value);

        var campaigns = await query.ToListAsync();

        return new CampaignSummaryDto
        {
            TotalCampaigns = campaigns.Count,
            ActiveCampaigns = campaigns.Count(c => c.IsActive && now >= c.StartDate && now <= c.EndDate),
            UpcomingCampaigns = campaigns.Count(c => c.IsActive && c.StartDate > now),
            ExpiredCampaigns = campaigns.Count(c => c.EndDate < now),
            CurrentCampaigns = campaigns
                .Where(c => c.IsActive && now >= c.StartDate && now <= c.EndDate)
                .Select(MapToDto)
                .OrderBy(c => c.EndDate)
                .Take(5)
                .ToList()
        };
    }

    public async Task<CampaignDto?> GetByIdAsync(int id)
    {
        var query = _context.Campaigns.Include(c => c.Company).AsNoTracking();
        var companyId = _tenantService.GetCompanyId();
        if (companyId.HasValue) query = query.Where(c => c.CompanyId == companyId.Value);
        
        var campaign = await query.FirstOrDefaultAsync(c => c.Id == id);
        return campaign == null ? null : MapToDto(campaign);
    }

    public async Task<CampaignDto> CreateAsync(CampaignCreateDto dto)
    {
        // Enforce tenant?
        var companyId = _tenantService.GetCompanyId() ?? dto.CompanyId;
        
        var campaign = Campaign.Create(
            dto.Name,
            dto.DiscountPercent,
            dto.StartDate,
            dto.EndDate,
            companyId,
            dto.Description);

        _context.Campaigns.Add(campaign);
        await _context.SaveChangesAsync();
        
        return MapToDto(campaign);
    }

    public async Task UpdateAsync(int id, CampaignUpdateDto dto)
    {
        var campaign = await GetCampaignEntityAsync(id);
        if (campaign == null) throw new KeyNotFoundException("Campaign not found");
        
        campaign.Update(dto.Name, dto.DiscountPercent, dto.StartDate, dto.EndDate, dto.Description);
        await _context.SaveChangesAsync();
    }

    public async Task ActivateAsync(int id)
    {
        var campaign = await GetCampaignEntityAsync(id);
        if (campaign == null) throw new KeyNotFoundException("Campaign not found");
        
        campaign.Activate();
        await _context.SaveChangesAsync();
    }

    public async Task DeactivateAsync(int id)
    {
        var campaign = await GetCampaignEntityAsync(id);
        if (campaign == null) throw new KeyNotFoundException("Campaign not found");
        
        campaign.Deactivate();
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var campaign = await GetCampaignEntityAsync(id);
        if (campaign == null) throw new KeyNotFoundException("Campaign not found");
        
        _context.Campaigns.Remove(campaign);
        await _context.SaveChangesAsync();
    }
    
    private static CampaignDto MapToDto(Campaign c)
    {
        var now = DateTime.UtcNow;
        return new CampaignDto
        {
            Id = c.Id,
            Name = c.Name,
            Description = c.Description,
            DiscountPercent = c.DiscountPercent,
            StartDate = c.StartDate,
            EndDate = c.EndDate,
            IsActive = c.IsActive,
            IsCurrentlyActive = c.IsActive && now >= c.StartDate && now <= c.EndDate,
            RemainingDays = c.IsActive && now >= c.StartDate && now <= c.EndDate
                ? (int)(c.EndDate - now).TotalDays
                : 0,
            CompanyId = c.CompanyId,
            CompanyName = c.Company?.Name ?? "",
            CreatedAt = c.CreatedAt
        };
    }
}
