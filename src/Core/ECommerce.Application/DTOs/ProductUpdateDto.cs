namespace ECommerce.Application.DTOs;

/// <summary>
/// Ürün güncelleme DTO
/// </summary>
public record ProductUpdateDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public int CategoryId { get; init; }
    public int BrandId { get; init; }
    public decimal Price { get; init; }
    public int StockQuantity { get; init; }
    public string? Sku { get; init; }
    public string? Barcode { get; init; }
    public string? ImageUrl { get; init; }
    public bool IsActive { get; init; } = true;
}
