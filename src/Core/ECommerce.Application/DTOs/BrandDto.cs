namespace ECommerce.Application.DTOs;

public record BrandDto(
    int Id,
    string Name,
    string Description,
    int CompanyId,
    string? ImageUrl = null,
    int? CategoryId = null,
    string? CategoryName = null,
    bool IsActive = true,
    int ProductCount = 0,
    DateTime? CreatedAt = null,
    DateTime? UpdatedAt = null
);