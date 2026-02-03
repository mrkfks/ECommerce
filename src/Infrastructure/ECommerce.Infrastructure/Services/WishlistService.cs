using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ECommerce.Infrastructure.Services;

/// <summary>
/// Wishlist service implementation
/// </summary>
public class WishlistService : IWishlistService
{
    private readonly AppDbContext _context;
    private readonly ILogger<WishlistService> _logger;
    private readonly ITenantService _tenantService;

    public WishlistService(AppDbContext context, ILogger<WishlistService> logger, ITenantService tenantService)
    {
        _context = context;
        _logger = logger;
        _tenantService = tenantService;
    }

    public async Task<WishlistDto?> GetWishlistAsync(string? sessionId)
    {
        try
        {
            _logger.LogInformation("GetWishlistAsync started with sessionId: {SessionId}", sessionId);

            Wishlist? wishlist = null;

            if (string.IsNullOrWhiteSpace(sessionId))
            {
                _logger.LogInformation("SessionId is null or whitespace, returning empty wishlist");
                return new WishlistDto { Items = new() };
            }

            _logger.LogInformation("Getting CompanyId from TenantService");
            // Get current company ID for tenant filtering
            int? companyId = _tenantService.GetCompanyId();
            _logger.LogInformation("CompanyId from TenantService: {CompanyId}", companyId);

            // Try to find wishlist by session ID
            var query = _context.Wishlists
                .Include(w => w.Items)
                .Where(w => w.SessionId == sessionId);

            // If we have a company ID, filter by it
            if (companyId.HasValue)
            {
                _logger.LogInformation("Filtering by CompanyId: {CompanyId}", companyId.Value);
                query = query.Where(w => w.CompanyId == companyId.Value);
            }

            _logger.LogInformation("Executing Wishlist query with AsNoTracking");
            wishlist = await query.AsNoTracking().FirstOrDefaultAsync();

            _logger.LogInformation("Wishlist retrieved: {WishlistId}", wishlist?.Id ?? 0);

            if (wishlist == null)
            {
                _logger.LogInformation("Wishlist is null, returning empty wishlist");
                return new WishlistDto { Items = new() };
            }

            // Manually load products for items
            var productIds = wishlist.Items.Select(i => i.ProductId).ToList();
            _logger.LogInformation("Loading products for {ProductCount} items", productIds.Count);

            var products = await _context.Products
                .Where(p => productIds.Contains(p.Id))
                .AsNoTracking()
                .ToListAsync();

            _logger.LogInformation("Products loaded: {ProductCount}", products.Count);

            return MapToDto(wishlist, products);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting wishlist for session {SessionId}: {ExceptionMessage}", sessionId, ex.Message);
            _logger.LogError("StackTrace: {StackTrace}", ex.StackTrace);
            throw;
        }
    }

    public async Task AddToWishlistAsync(int productId, string? sessionId)
    {
        try
        {
            // Ürün var mı kontrol et
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
            {
                throw new KeyNotFoundException($"Ürün {productId} bulunamadı");
            }

            int? companyId = _tenantService.GetCompanyId();
            if (!companyId.HasValue)
            {
                throw new InvalidOperationException("Company context not found");
            }

            // Wishlist bul veya oluştur
            Wishlist? wishlist = null;

            if (!string.IsNullOrWhiteSpace(sessionId))
            {
                wishlist = await _context.Wishlists
                    .Include(w => w.Items)
                    .FirstOrDefaultAsync(w => w.SessionId == sessionId && w.CompanyId == companyId.Value);
            }

            if (wishlist == null)
            {
                wishlist = new Wishlist
                {
                    SessionId = sessionId,
                    CompanyId = companyId.Value,
                    Items = new()
                };
                _context.Wishlists.Add(wishlist);
                await _context.SaveChangesAsync();
            }

            // Aynı ürün zaten varsa ekleme
            var existingItem = wishlist.Items.FirstOrDefault(i => i.ProductId == productId);
            if (existingItem != null)
            {
                _logger.LogInformation("Product {ProductId} already in wishlist for session {SessionId}", productId, sessionId);
                return;
            }

            // Wishlist'e ürün ekle
            var item = new WishlistItem
            {
                WishlistId = wishlist.Id,
                ProductId = productId
            };

            _context.WishlistItems.Add(item);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Product {ProductId} added to wishlist for session {SessionId}", productId, sessionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding product {ProductId} to wishlist", productId);
            throw;
        }
    }

    public async Task RemoveFromWishlistAsync(int itemId)
    {
        try
        {
            var item = await _context.WishlistItems.FindAsync(itemId);
            if (item == null)
            {
                throw new KeyNotFoundException($"Wishlist item {itemId} bulunamadı");
            }

            _context.WishlistItems.Remove(item);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Wishlist item {ItemId} removed", itemId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing wishlist item {ItemId}", itemId);
            throw;
        }
    }

    public async Task ClearWishlistAsync(string? sessionId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(sessionId))
            {
                return;
            }

            int? companyId = _tenantService.GetCompanyId();
            if (!companyId.HasValue)
            {
                throw new InvalidOperationException("Company context not found");
            }

            var wishlist = await _context.Wishlists
                .Include(w => w.Items)
                .FirstOrDefaultAsync(w => w.SessionId == sessionId && w.CompanyId == companyId.Value);

            if (wishlist == null)
            {
                return;
            }

            _context.WishlistItems.RemoveRange(wishlist.Items);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Wishlist cleared for session {SessionId}", sessionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing wishlist for session {SessionId}", sessionId);
            throw;
        }
    }

    private WishlistDto MapToDto(Wishlist wishlist, List<Product>? products = null)
    {
        products ??= new();
        var productDict = products.ToDictionary(p => p.Id);

        return new WishlistDto
        {
            Id = wishlist.Id,
            Items = wishlist.Items.Select(i =>
            {
                var product = productDict.TryGetValue(i.ProductId, out var p) ? p : null;
                return new WishlistItemDto
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    ProductName = product?.Name ?? string.Empty,
                    ProductImage = product?.ImageUrl,
                    Price = product?.Price ?? 0,
                    CompanyId = product?.CompanyId ?? 0,
                    CreatedAt = i.CreatedAt
                };
            }).ToList(),
            TotalItems = wishlist.Items.Count
        };
    }
}
