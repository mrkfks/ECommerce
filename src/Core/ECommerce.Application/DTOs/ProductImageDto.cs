namespace ECommerce.Application.DTOs;

/// <summary>
/// Ürün resmi DTO
/// </summary>
public record ProductImageDto
{
    public int Id { get; init; }
    public int ProductId { get; init; }
    public string ImageUrl { get; init; } = string.Empty;
    public int DisplayOrder { get; init; }
    public int Order { get; init; } // Geriye uyumluluk
    public bool IsMain { get; init; }
    public bool IsPrimary { get; init; } // Geriye uyumluluk
}
