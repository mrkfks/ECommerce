namespace ECommerce.Application.DTOs;

public record CategoryDto(
    int Id,
    string Name,
    string Description,
    string? ImageUrl = null,
    bool IsActive = true,
    int ProductCount = 0,
    int? ParentCategoryId = null,
    int DisplayOrder = 0,
    DateTime? CreatedAt = null,
    DateTime? UpdatedAt = null
);