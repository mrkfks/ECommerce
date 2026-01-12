using System.Security.Claims;
using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Services;

public class CartService : ICartService
{
    private readonly AppDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ITenantService _tenantService;

    public CartService(AppDbContext context, IHttpContextAccessor httpContextAccessor, ITenantService tenantService)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _tenantService = tenantService;
    }

    public async Task<CartDto> GetCartAsync(string? sessionId = null)
    {
        var cart = await GetOrCreateCartAsync(sessionId);
        return MapToDto(cart);
    }

    public async Task AddToCartAsync(AddToCartDto dto, string? sessionId = null)
    {
        var cart = await GetOrCreateCartAsync(sessionId);
        var product = await _context.Products.FindAsync(dto.ProductId);
        
        if (product == null)
            throw new Application.Exceptions.NotFoundException("Ürün bulunamadı");
        
        // TODO: Check stock, etc.

        cart.AddItem(dto.ProductId, dto.Quantity, product.Price); // Using Product Price as Unit Price
        await _context.SaveChangesAsync();
    }

    public async Task RemoveFromCartAsync(int itemId)
    {
        // Security check: item belongs to current cart?
        // Simplifying for MVP: just find item and remove if user owns the cart.
        
        var cart = await GetOrCreateCartAsync(null); // Get current context cart
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
                cart.Items.Remove(item);
            else
                item.UpdateQuantity(quantity);
                
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
        // Logic to move guest cart items to user cart upon login
        // 1. Get Guest Cart
        var guestCart = await _context.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.SessionId == sessionId && c.Customer == null);

        if (guestCart == null || !guestCart.Items.Any())
            return;

        // 2. Get/Create User Cart
        var userCart = await GetOrCreateCartAsync(null); // Will get user cart since we are logged in

        // 3. Merge Items
        foreach (var item in guestCart.Items)
        {
            userCart.AddItem(item.ProductId, item.Quantity, item.UnitPrice);
        }

        // 4. Delete guest cart or clear it
        _context.Carts.Remove(guestCart);
        await _context.SaveChangesAsync();
    }

    private async Task<Cart> GetOrCreateCartAsync(string? sessionId)
    {
        var userIdStr = _httpContextAccessor.HttpContext?.User?.FindFirst("userId")?.Value; // Claim types may vary
        // Fallback to standard claim types if custom one fails
        if (string.IsNullOrEmpty(userIdStr))
            userIdStr = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        int? currentUserId = null;
        if (int.TryParse(userIdStr, out int uid))
        {
            currentUserId = uid;
        }
        else 
        {
             // Try getting from token manually if needed or assume guest
        }

        int companyId = _tenantService.GetCompanyId() ?? 2; // Default system company or error?

        Cart? cart = null;

        // 1. Authenticated User
        if (currentUserId.HasValue)
        {
            // Find Customer for this User
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == currentUserId.Value);
            if (customer == null)
            {
                // Should we create a customer profile? Or just fail?
                // For MVP, if no customer profile, maybe we can't have a cart? 
                // Creating a simplified path: Assume Customer exists or logic handles it in other services.
                // Or maybe Cart can have UserId? The Entity has CustomerId.
                // Let's create a Customer if not exists? Or just fallback.
            }

            if (customer != null)
            {
                cart = await _context.Carts
                    .Include(c => c.Items).ThenInclude(i => i.Product)
                    .FirstOrDefaultAsync(c => c.CustomerId == customer.Id);

                if (cart == null)
                {
                    cart = Cart.Create(companyId, customer.Id, null);
                    _context.Carts.Add(cart);
                    await _context.SaveChangesAsync();
                }
                return cart;
            }
        }

        // 2. Guest User (Session)
        // Ensure sessionId provided
        if (string.IsNullOrEmpty(sessionId))
        {
            // Try to look for session header if not passed explicitly?
            if (_httpContextAccessor.HttpContext?.Request.Headers.TryGetValue("X-Session-ID", out var headerSessionId) == true)
            {
                sessionId = headerSessionId.ToString();
            }
            
            // Generate if strictly missing?
            if (string.IsNullOrEmpty(sessionId)) throw new System.Exception("Session ID required for guest cart");
        }

        cart = await _context.Carts
             .Include(c => c.Items).ThenInclude(i => i.Product)
             .FirstOrDefaultAsync(c => c.SessionId == sessionId && c.CustomerId == null);

        if (cart == null)
        {
            cart = Cart.Create(companyId, null, sessionId);
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
