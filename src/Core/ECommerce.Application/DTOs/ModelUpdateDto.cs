namespace ECommerce.Application.DTOs;

/// <summary>
/// Model güncelleme DTO
/// </summary>
public record ModelUpdateDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public int BrandId { get; init; }
    public string? Description { get; init; }
    public bool IsActive { get; init; } = true;
}
