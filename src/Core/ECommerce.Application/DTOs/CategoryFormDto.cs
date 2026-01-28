namespace ECommerce.Application.DTOs;

/// <summary>
/// Kategori form DTO
/// </summary>
public record CategoryFormDto
{
    public int? Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? ImageUrl { get; init; }
    public int? ParentCategoryId { get; init; }
    public int DisplayOrder { get; init; }
    public bool IsActive { get; init; } = true;
}
