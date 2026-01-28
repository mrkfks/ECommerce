namespace ECommerce.Application.DTOs;

/// <summary>
/// Kategori bilgisi DTO
/// </summary>
public record CategoryDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string? ImageUrl { get; init; }
    public bool IsActive { get; init; } = true;
    public int ProductCount { get; init; }
    public int? ParentCategoryId { get; init; }
    public int DisplayOrder { get; init; }
    public DateTime? CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}