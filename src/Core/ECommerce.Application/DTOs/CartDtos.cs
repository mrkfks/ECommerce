namespace ECommerce.Application.DTOs;

/// <summary>
/// Sepet bilgisi DTO
/// </summary>
public record CartDto
{
    public int Id { get; init; }
    public decimal TotalAmount { get; init; }
    public List<CartItemDto> Items { get; init; } = new();
}

/// <summary>
/// Sepet kalemi DTO
/// </summary>
public record CartItemDto
{
    public int Id { get; init; }
    public int ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public string? ImageUrl { get; init; }
    public string? ProductImage { get; init; }  // Geriye uyumluluk
    public int Quantity { get; init; }
    public decimal UnitPrice { get; init; }
    public decimal TotalPrice { get; init; }
    public int StockQuantity { get; init; }
    public int CompanyId { get; init; }
}

/// <summary>
/// Sepete ürün ekleme DTO
/// </summary>
public record AddToCartDto
{
    public int ProductId { get; init; }
    public int Quantity { get; init; } = 1;
}

/// <summary>
/// Sepet kalemi güncelleme DTO
/// </summary>
public record UpdateCartItemDto
{
    public int CartItemId { get; init; }
    public int Quantity { get; init; }
}
