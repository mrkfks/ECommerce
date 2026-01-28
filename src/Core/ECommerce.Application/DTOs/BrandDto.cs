namespace ECommerce.Application.DTOs;

/// <summary>
/// Marka bilgisi DTO
/// </summary>
public record BrandDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public int CompanyId { get; init; }
    public string? ImageUrl { get; init; }
    public int? CategoryId { get; init; }
    public string? CategoryName { get; init; }
    public bool IsActive { get; init; } = true;
    public int ProductCount { get; init; }
    public DateTime? CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}