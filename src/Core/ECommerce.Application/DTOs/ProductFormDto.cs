namespace ECommerce.Application.DTOs;

/// <summary>
/// Ürün oluşturma/güncelleme form DTO
/// </summary>
public record ProductFormDto
{
    public int? Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public decimal Price { get; init; }
    public int StockQuantity { get; init; }
    public int CategoryId { get; init; }
    public int BrandId { get; init; }
    public int CompanyId { get; init; }
    public int? ModelId { get; init; }
    public string? ImageUrl { get; init; }
    public bool IsActive { get; init; } = true;
}
