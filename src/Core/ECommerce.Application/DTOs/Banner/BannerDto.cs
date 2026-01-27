namespace ECommerce.Application.DTOs.Banner;

public record BannerDto(
    int Id,
    string Title,
    string? Description,
    string ImageUrl,
    string? Link,
    int Order,
    bool IsActive,
    int CompanyId,
    DateTime CreatedAt
);

public record BannerFormDto(
    int? Id,
    string Title,
    string ImageUrl,
    string? Description = null,
    string? Link = null,
    int Order = 0,
    bool IsActive = true,
    int? CompanyId = null
);
