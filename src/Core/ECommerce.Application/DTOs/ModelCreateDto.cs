namespace ECommerce.Application.DTOs;

/// <summary>
/// Model oluşturma DTO
/// </summary>
public record ModelCreateDto
{
    public string Name { get; init; } = string.Empty;
    public int BrandId { get; init; }
    public string? Description { get; init; }
    public bool IsActive { get; init; } = true;
}
