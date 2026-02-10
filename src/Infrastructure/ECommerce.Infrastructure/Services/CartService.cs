using System.Security.Claims;
using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ECommerce.Infrastructure.Services;

public class CartService : ICartService
{
    private readonly AppDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ITenantService _tenantService;
    private readonly ILogger<CartService> _logger;

    public CartService(AppDbContext context,
        IHttpContextAccessor httpContextAccessor,
        ITenantService tenantService,
        ILogger<CartService> logger)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _tenantService = tenantService;
        _logger = logger;
    }

    public async Task<CartDto> GetCartAsync(string? sessionId = null)
    {
        var cart = await GetOrCreateCartAsync(sessionId);
        return MapToDto(cart);
    }

    public async Task AddToCartAsync(AddToCartDto dto, string? sessionId = null)
    {
        var product = await _context.Products.FindAsync(dto.ProductId);

        if (product == null)
            throw new Application.Exceptions.NotFoundException("Ürün bulunamadı");

        if (!product.IsActive)
            throw new Application.Exceptions.BadRequestException("Bu ürün şu anda satışta değil.");

        // Stok Kontrolü
        if (product.StockQuantity < dto.Quantity)
            throw new Application.Exceptions.BadRequestException($"Yetersiz stok. Mevcut stok: {product.StockQuantity}");

        var cart = await GetOrCreateCartAsync(sessionId);

        // Eğer ürün zaten sepetteyse toplam miktarı da kontrol etmeliyiz
        var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == dto.ProductId);
        if (existingItem != null)
        {
            if (product.StockQuantity < (existingItem.Quantity + dto.Quantity))
                throw new Application.Exceptions.BadRequestException($"Sepetinizdeki miktar ile birlikte stok sınırı aşılıyor. Mevcut stok: {product.StockQuantity}");
        }

        cart.AddItem(dto.ProductId, dto.Quantity, product.Price);
        await _context.SaveChangesAsync();
    }

    public async Task RemoveFromCartAsync(int itemId)
    {
        var cart = await GetOrCreateCartAsync(null);
        var item = cart.Items.FirstOrDefault(i => i.Id == itemId);

        if (item != null)
        {
            cart.Items.Remove(item);
            await _context.SaveChangesAsync();
        }
    }

    public async Task UpdateQuantityAsync(int itemId, int quantity)
    {
        var cart = await GetOrCreateCartAsync(null);
        var item = cart.Items.FirstOrDefault(i => i.Id == itemId);

        if (item != null)
        {
            if (quantity <= 0)
            {
                cart.Items.Remove(item);
            }
            else
            {
                // Stok kontrolü (Güncellenirken)
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product != null && product.StockQuantity < quantity)
                    throw new Application.Exceptions.BadRequestException($"Yetersiz stok. Mevcut stok: {product.StockQuantity}");

                item.UpdateQuantity(quantity);
            }

            await _context.SaveChangesAsync();
        }
    }

    public async Task ClearCartAsync()
    {
        var cart = await GetOrCreateCartAsync(null);
        cart.ClearItems();
        await _context.SaveChangesAsync();
    }

    public async Task MergeCartAsync(string sessionId)
    {
        var guestCart = await _context.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.SessionId == sessionId && c.Customer == null);

        if (guestCart == null || !guestCart.Items.Any())
            return;

        var userCart = await GetOrCreateCartAsync(null);

        foreach (var item in guestCart.Items)
        {
            userCart.AddItem(item.ProductId, item.Quantity, item.UnitPrice);
        }

        _context.Carts.Remove(guestCart);
        await _context.SaveChangesAsync();
    }

    private async Task<Cart> GetOrCreateCartAsync(string? sessionId)
    {
        var userIdStr = _httpContextAccessor.HttpContext?.User?.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(userIdStr))
            userIdStr = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        int? currentUserId = null;
        if (int.TryParse(userIdStr, out int uid))
        {
            currentUserId = uid;
        }
        int? companyId = _tenantService.GetCompanyId();

        if (!companyId.HasValue)
        {
            // Eğer hala companyId yoksa, sepet işlemini gerçekleştiremeyiz.
            // Çünkü sepet hangi dükkana/organizasyona ait bilinmeli.
            throw new Application.Exceptions.BadRequestException("Şirket bilgisi eksik. Lütfen siteyi doğru kanal üzerinden ziyaret ettiğinizden emin olun.");
        }

        // Şirketin varlığını kontrol et (Geçersiz veya eski ID gönderilmesini engellemek için)
        var companyExists = await _context.Companies.AnyAsync(c => c.Id == companyId.Value);
        if (!companyExists)
        {
            throw new Application.Exceptions.BadRequestException("Geçersiz şirket bilgisi. Lütfen sayfayı yenileyiniz.");
        }

        Cart? cart = null;

        if (currentUserId.HasValue)
        {
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == currentUserId.Value);
            if (customer != null)
            {
                cart = await _context.Carts
                    .Include(c => c.Items).ThenInclude(i => i.Product)
                    .FirstOrDefaultAsync(c => c.CustomerId == customer.Id);

                if (cart == null)
                {
                    cart = Cart.Create(companyId.Value, customer.Id, null);
                    _context.Carts.Add(cart);
                    await _context.SaveChangesAsync();
                }
                return cart;
            }
        }

        if (string.IsNullOrEmpty(sessionId))
        {
            if (_httpContextAccessor.HttpContext?.Request.Headers.TryGetValue("X-Session-ID", out var headerSessionId) == true)
            {
                sessionId = headerSessionId.ToString();
            }

            if (string.IsNullOrEmpty(sessionId))
                throw new Application.Exceptions.BadRequestException("Misafir sepeti için Session ID gereklidir.");
        }

        cart = await _context.Carts
             .Include(c => c.Items).ThenInclude(i => i.Product)
             .FirstOrDefaultAsync(c => c.SessionId == sessionId && c.CustomerId == null);

        if (cart == null)
        {
            cart = Cart.Create(companyId.Value, null, sessionId);
            _context.Carts.Add(cart);
            await _context.SaveChangesAsync();
        }

        return cart;
    }

    private CartDto MapToDto(Cart cart)
    {
        return new CartDto
        {
            Id = cart.Id,
            TotalAmount = cart.TotalAmount,
            Items = cart.Items.Select(i => new CartItemDto
            {
                Id = i.Id,
                ProductId = i.ProductId,
                ProductName = i.Product?.Name ?? "Unknown",
                ProductImage = i.Product?.ImageUrl ?? "", // Simplified
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                TotalPrice = i.TotalPrice,
                CompanyId = i.Product?.CompanyId ?? 0
            }).ToList()
        };
    }
}
