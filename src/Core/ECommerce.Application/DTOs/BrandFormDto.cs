namespace ECommerce.Application.DTOs;

public record BrandFormDto(
    int? Id,
    string Name,
    string Description,
    string? ImageUrl = null,
    int? CategoryId = null,
    int? CompanyId = null,
    bool IsActive = true
);
