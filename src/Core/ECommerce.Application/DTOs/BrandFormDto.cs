namespace ECommerce.Application.DTOs;

/// <summary>
/// Marka form DTO
/// </summary>
public record BrandFormDto
{
    public int? Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string? ImageUrl { get; init; }
    public int? CategoryId { get; init; }
    public int? CompanyId { get; init; }
    public bool IsActive { get; init; } = true;
}
