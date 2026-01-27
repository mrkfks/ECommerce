namespace ECommerce.Application.DTOs;

public record ModelDto(
    int Id,
    int BrandId,
    string? BrandName,
    string Name,
    string Description,
    string? ImageUrl = null,
    bool IsActive = true,
    DateTime? CreatedAt = null,
    DateTime? UpdatedAt = null
);

public record ModelFormDto(
    int? Id,
    int BrandId,
    string Name,
    string Description,
    string? ImageUrl = null,
    bool IsActive = true
);
