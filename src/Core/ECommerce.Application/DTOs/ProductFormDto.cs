namespace ECommerce.Application.DTOs;

public record ProductFormDto(
    int? Id,
    string Name,
    string Description,
    decimal Price,
    int StockQuantity,
    int CategoryId,
    int BrandId,
    int CompanyId,
    int? ModelId = null,
    string? ImageUrl = null,
    bool IsActive = true
);
