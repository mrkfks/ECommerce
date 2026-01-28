namespace ECommerce.Application.DTOs;

/// <summary>
/// Kampanya güncelleme DTO
/// </summary>
public record CampaignUpdateDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public decimal DiscountPercent { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public decimal DiscountRate { get; init; } // Geriye uyumluluk için
}
