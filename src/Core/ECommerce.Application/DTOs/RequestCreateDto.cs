namespace ECommerce.Application.DTOs;

/// <summary>
/// Talep oluşturma DTO
/// </summary>
public record RequestCreateDto
{
    public int CompanyId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public int CreatedByUserId { get; init; }
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}
