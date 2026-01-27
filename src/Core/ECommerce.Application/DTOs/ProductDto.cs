namespace ECommerce.Application.DTOs;

public record ProductDto(
    int Id,
    string Name,
    string Description,
    decimal Price,
    int StockQuantity,
    int CategoryId,
    string CategoryName,
    int BrandId,
    string BrandName,
    int CompanyId,
    string CompanyName,
    int? ModelId = null,
    string? ModelName = null,
    string? ImageUrl = null,
    bool IsActive = true,
    DateTime? CreatedAt = null,
    DateTime? UpdatedAt = null,
    int ReviewCount = 0,
    double AverageRating = 0,
    List<ProductImageDto>? Images = null
);
