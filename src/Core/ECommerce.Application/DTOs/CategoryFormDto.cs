namespace ECommerce.Application.DTOs;

public record CategoryFormDto(
    int? Id,
    string Name,
    string? Description = null,
    string? ImageUrl = null,
    int? ParentCategoryId = null,
    int DisplayOrder = 0,
    bool IsActive = true
);
