namespace ECommerce.Application.DTOs;

/// <summary>
/// Marka oluşturma DTO
/// </summary>
public record BrandCreateDto
{
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? LogoUrl { get; init; }
    public string? WebsiteUrl { get; init; }
    public int? CountryId { get; init; }
    public bool IsActive { get; init; } = true;
}

/// <summary>
/// Marka güncelleme DTO
/// </summary>
public record BrandUpdateDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? LogoUrl { get; init; }
    public string? WebsiteUrl { get; init; }
    public int? CountryId { get; init; }
    public bool IsActive { get; init; } = true;
}
