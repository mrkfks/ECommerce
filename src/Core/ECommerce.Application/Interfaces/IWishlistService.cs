using ECommerce.Application.DTOs;

namespace ECommerce.Application.Interfaces;

/// <summary>
/// Wishlist (Favoriler) service interface
/// </summary>
public interface IWishlistService
{
    /// <summary>
    /// Session veya user'ın wishlist'ini getir
    /// </summary>
    Task<WishlistDto?> GetWishlistAsync(string? sessionId);

    /// <summary>
    /// Wishlist'e ürün ekle
    /// </summary>
    Task AddToWishlistAsync(int productId, string? sessionId);

    /// <summary>
    /// Wishlist'ten ürünü kaldır
    /// </summary>
    Task RemoveFromWishlistAsync(int itemId);

    /// <summary>
    /// Wishlist'i temizle
    /// </summary>
    Task ClearWishlistAsync(string? sessionId);
}

/// <summary>
/// Wishlist DTO
/// </summary>
public record WishlistDto
{
    public int Id { get; init; }
    public List<WishlistItemDto> Items { get; init; } = new();
    public int TotalItems { get; init; }
}

/// <summary>
/// Wishlist item DTO
/// </summary>
public record WishlistItemDto
{
    public int Id { get; init; }
    public int ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public string? ProductImage { get; init; }
    public decimal Price { get; init; }
    public int CompanyId { get; init; }
    public DateTime CreatedAt { get; init; }
}
