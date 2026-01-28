namespace ECommerce.Application.DTOs;

/// <summary>
/// Ürün bilgisi DTO
/// </summary>
public record ProductDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public int StockQuantity { get; init; }
    public int CategoryId { get; init; }
    public string CategoryName { get; init; } = string.Empty;
    public int BrandId { get; init; }
    public string BrandName { get; init; } = string.Empty;
    public int CompanyId { get; init; }
    public string CompanyName { get; init; } = string.Empty;
    public int? ModelId { get; init; }
    public string? ModelName { get; init; }
    public string? ImageUrl { get; init; }
    public bool IsActive { get; init; } = true;
    public DateTime? CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public int ReviewCount { get; init; }
    public double AverageRating { get; init; }
    public List<ProductImageDto>? Images { get; init; }
}

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

/// <summary>
/// Ürün oluşturma DTO (ProductFormDto'nun alias'ı - geriye uyumluluk için)
/// </summary>
public record ProductCreateDto
{
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
