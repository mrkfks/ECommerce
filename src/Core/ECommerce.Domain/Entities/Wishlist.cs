namespace ECommerce.Domain.Entities;

/// <summary>
/// Wishlist (Favoriler) entity
/// </summary>
public class Wishlist : BaseEntity, ITenantEntity
{
    public int? UserId { get; set; }
    public string? SessionId { get; set; }
    public int CompanyId { get; set; }

    // Navigation Properties
    public User? User { get; set; }
    public Company? Company { get; set; }
    public List<WishlistItem> Items { get; set; } = new();
}

/// <summary>
/// Wishlist item entity
/// </summary>
public class WishlistItem : BaseEntity
{
    public int WishlistId { get; set; }
    public int ProductId { get; set; }

    // Navigation Properties
    public Wishlist? Wishlist { get; set; }
    public Product? Product { get; set; }
}
