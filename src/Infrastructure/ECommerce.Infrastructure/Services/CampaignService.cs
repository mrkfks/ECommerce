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

    public async Task<CampaignDto> CreateAsync(CampaignFormDto dto)
    {
        // Enforce tenant?
        var companyId = _tenantService.GetCompanyId() ?? dto.CompanyId ?? 1;

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

    public async Task UpdateAsync(int id, CampaignFormDto dto)
    {
        var campaign = await GetCampaignEntityAsync(id);
        if (campaign == null) throw new KeyNotFoundException("Campaign not found");

        campaign.Update(dto.Name, dto.DiscountPercent, dto.StartDate, dto.EndDate, dto.Description, dto.BannerImageUrl);
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
        if (campaign == null) 
            throw new KeyNotFoundException("Kampanya bulunamadı veya bu kampayaya erişim yetkiniz yok.");

        try
        {
            // Önce ilişkili ProductCampaign kayıtlarını sil
            var productCampaigns = await _context.ProductCampaigns
                .Where(pc => pc.CampaignId == id)
                .ToListAsync();
            
            if (productCampaigns.Any())
            {
                _context.ProductCampaigns.RemoveRange(productCampaigns);
                _logger.LogInformation("Removed {Count} ProductCampaign records for Campaign ID: {CampaignId}", 
                    productCampaigns.Count, id);
            }

            _context.Campaigns.Remove(campaign);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Campaign ID: {CampaignId} deleted successfully", id);
        }
        catch (DbUpdateException ex)
        {
            // Foreign key constraint veya başka veritabanı hatası
            _logger.LogError(ex, "Failed to delete campaign ID: {CampaignId}", id);
            throw new InvalidOperationException("Kampanya silinemedi. Lütfen ilgili ürünleri veya diğer bağlantıları kontrol edin: " + ex.InnerException?.Message, ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error deleting campaign ID: {CampaignId}", id);
            throw new InvalidOperationException("Kampanya silinirken beklenmeyen bir hata oluştu: " + ex.Message, ex);
        }
    }

    private static CampaignDto MapToDto(Campaign c)
    {
        var now = DateTime.UtcNow;
        return new CampaignDto
        {
            Id = c.Id,
            Name = c.Name,
            Description = c.Description,
            BannerImageUrl = c.BannerImageUrl,
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

    // Category-based Campaign Methods

    public async Task<List<int>> GetCampaignCategoryIdsAsync(int campaignId)
    {
        var campaign = await GetCampaignEntityAsync(campaignId);
        if (campaign == null)
            throw new KeyNotFoundException($"Kampanya bulunamadı: {campaignId}");

        return await _context.CategoryCampaigns
            .Where(cc => cc.CampaignId == campaignId)
            .Select(cc => cc.CategoryId)
            .ToListAsync();
    }

    public async Task AddCategoriesToCampaignAsync(int campaignId, List<int> categoryIds)
    {
        if (categoryIds == null || !categoryIds.Any())
            throw new ArgumentException("En az bir kategori seçilmelidir.");

        var campaign = await GetCampaignEntityAsync(campaignId);
        if (campaign == null)
            throw new KeyNotFoundException($"Kampanya bulunamadı: {campaignId}");

        // Verify categories exist
        var existingCategories = await _context.Categories
            .Where(c => categoryIds.Contains(c.Id))
            .Select(c => c.Id)
            .ToListAsync();

        var invalidIds = categoryIds.Except(existingCategories).ToList();
        if (invalidIds.Any())
            throw new ArgumentException($"Geçersiz kategori ID'leri: {string.Join(", ", invalidIds)}");

        // Get already added categories
        var existingCategoryCampaigns = await _context.CategoryCampaigns
            .Where(cc => cc.CampaignId == campaignId && categoryIds.Contains(cc.CategoryId))
            .Select(cc => cc.CategoryId)
            .ToListAsync();

        // Add only new categories
        var newCategoryIds = categoryIds.Except(existingCategoryCampaigns).ToList();
        
        foreach (var categoryId in newCategoryIds)
        {
            var categoryCampaign = CategoryCampaign.Create(categoryId, campaignId);
            await _context.CategoryCampaigns.AddAsync(categoryCampaign);
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("Added {Count} categories to campaign {CampaignId}", newCategoryIds.Count, campaignId);
    }

    public async Task RemoveCategoryFromCampaignAsync(int campaignId, int categoryId)
    {
        var categoryCampaign = await _context.CategoryCampaigns
            .FirstOrDefaultAsync(cc => cc.CampaignId == campaignId && cc.CategoryId == categoryId);

        if (categoryCampaign == null)
            throw new KeyNotFoundException($"Kategori {categoryId} kampanya {campaignId}'de bulunamadı.");

        _context.CategoryCampaigns.Remove(categoryCampaign);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Removed category {CategoryId} from campaign {CampaignId}", categoryId, campaignId);
    }
}
