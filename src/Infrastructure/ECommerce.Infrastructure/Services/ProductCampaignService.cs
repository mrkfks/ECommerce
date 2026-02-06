using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ECommerce.Infrastructure.Services;

public class ProductCampaignService : IProductCampaignService
{
    private readonly AppDbContext _context;
    private readonly ITenantService _tenantService;
    private readonly ILogger<ProductCampaignService> _logger;

    public ProductCampaignService(AppDbContext context, ITenantService tenantService, ILogger<ProductCampaignService> logger)
    {
        _context = context;
        _tenantService = tenantService;
        _logger = logger;
    }

    public async Task<ProductCampaignDto> AddProductToCampaignAsync(ProductCampaignFormDto dto)
    {
        // Ürün ve Kampanya var mı kontrol et
        var product = await _context.Products.FindAsync(dto.ProductId);
        if (product == null) throw new KeyNotFoundException($"Ürün {dto.ProductId} bulunamadı.");

        var campaign = await _context.Campaigns.FindAsync(dto.CampaignId);
        if (campaign == null) throw new KeyNotFoundException($"Kampanya {dto.CampaignId} bulunamadı.");

        // Zaten varsa kontrol et
        var existing = await _context.ProductCampaigns
            .FirstOrDefaultAsync(pc => pc.ProductId == dto.ProductId && pc.CampaignId == dto.CampaignId);

        if (existing != null)
            throw new InvalidOperationException($"Ürün {dto.ProductId} kampanya {dto.CampaignId}'de zaten mevcut.");

        var productCampaign = ProductCampaign.Create(
            dto.ProductId,
            dto.CampaignId,
            dto.OriginalPrice,
            dto.DiscountedPrice);

        _context.ProductCampaigns.Add(productCampaign);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Ürün {ProductId} kampanya {CampaignId}'ye eklendi.", dto.ProductId, dto.CampaignId);

        return await MapToDtoAsync(productCampaign);
    }

    public async Task RemoveProductFromCampaignAsync(int productId, int campaignId)
    {
        var productCampaign = await _context.ProductCampaigns
            .FirstOrDefaultAsync(pc => pc.ProductId == productId && pc.CampaignId == campaignId);

        if (productCampaign == null)
            throw new KeyNotFoundException($"Ürün {productId} ve kampanya {campaignId} ilişkisi bulunamadı.");

        _context.ProductCampaigns.Remove(productCampaign);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Ürün {ProductId} kampanya {CampaignId}'den kaldırıldı.", productId, campaignId);
    }

    public async Task<IReadOnlyList<ProductCampaignDto>> GetActiveByProductIdAsync(int productId)
    {
        var now = DateTime.UtcNow;
        var activeCampaigns = await _context.ProductCampaigns
            .Include(pc => pc.Product)
            .Include(pc => pc.Campaign)
            .Where(pc => pc.ProductId == productId
                && pc.Campaign!.IsActive
                && pc.Campaign.StartDate <= now
                && pc.Campaign.EndDate >= now)
            .ToListAsync();

        return await Task.WhenAll(activeCampaigns.Select(MapToDtoAsync)).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<ProductCampaignDto>> GetByCampaignIdAsync(int campaignId)
    {
        var products = await _context.ProductCampaigns
            .Include(pc => pc.Product)
            .Include(pc => pc.Campaign)
            .Where(pc => pc.CampaignId == campaignId)
            .ToListAsync();

        return await Task.WhenAll(products.Select(MapToDtoAsync)).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<ProductCampaignDto>> GetCurrentActiveCampaignsAsync(int companyId)
    {
        var now = DateTime.UtcNow;
        var activeCampaigns = await _context.ProductCampaigns
            .Include(pc => pc.Product)
            .Include(pc => pc.Campaign)
            .Where(pc => pc.Campaign!.CompanyId == companyId
                && pc.Campaign.IsActive
                && pc.Campaign.StartDate <= now
                && pc.Campaign.EndDate >= now)
            .ToListAsync();

        return await Task.WhenAll(activeCampaigns.Select(MapToDtoAsync)).ConfigureAwait(false);
    }

    public async Task UpdatePricesAsync(int productId, int campaignId, ProductCampaignFormDto dto)
    {
        var productCampaign = await _context.ProductCampaigns
            .FirstOrDefaultAsync(pc => pc.ProductId == productId && pc.CampaignId == campaignId);

        if (productCampaign == null)
            throw new KeyNotFoundException($"Ürün {productId} ve kampanya {campaignId} ilişkisi bulunamadı.");

        productCampaign.UpdatePrices(dto.OriginalPrice, dto.DiscountedPrice);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Ürün {ProductId} kampanya {CampaignId} fiyatları güncellendi.", productId, campaignId);
    }

    public async Task<ProductCampaignDto?> GetByIdAsync(int productId, int campaignId)
    {
        var productCampaign = await _context.ProductCampaigns
            .Include(pc => pc.Product)
            .Include(pc => pc.Campaign)
            .FirstOrDefaultAsync(pc => pc.ProductId == productId && pc.CampaignId == campaignId);

        if (productCampaign == null) return null;

        return await MapToDtoAsync(productCampaign);
    }

    private async Task<ProductCampaignDto> MapToDtoAsync(ProductCampaign pc)
    {
        var now = DateTime.UtcNow;
        var discountPercentage = pc.GetDiscountPercentage();

        return new ProductCampaignDto
        {
            ProductId = pc.ProductId,
            CampaignId = pc.CampaignId,
            ProductName = pc.Product?.Name ?? string.Empty,
            CampaignName = pc.Campaign?.Name ?? string.Empty,
            OriginalPrice = pc.OriginalPrice,
            DiscountedPrice = pc.DiscountedPrice,
            DiscountPercentage = (decimal)Math.Round(discountPercentage, 2),
            CampaignStartDate = pc.Campaign?.StartDate ?? DateTime.MinValue,
            CampaignEndDate = pc.Campaign?.EndDate ?? DateTime.MinValue,
            IsCampaignActive = pc.Campaign != null && pc.Campaign.IsActive
                && now >= pc.Campaign.StartDate
                && now <= pc.Campaign.EndDate,
            CreatedAt = pc.CreatedAt
        };
    }
}
