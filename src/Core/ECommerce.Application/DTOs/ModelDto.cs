namespace ECommerce.Application.DTOs;

/// <summary>
/// Model bilgisi DTO
/// </summary>
public record ModelDto
{
    public int Id { get; init; }
    public int BrandId { get; init; }
    public string? BrandName { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? ImageUrl { get; init; }
    public bool IsActive { get; init; } = true;
    public DateTime? CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}

/// <summary>
/// Model form DTO
/// </summary>
public record ModelFormDto
{
    public int? Id { get; init; }
    public int BrandId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? ImageUrl { get; init; }
    public bool IsActive { get; init; } = true;
}
