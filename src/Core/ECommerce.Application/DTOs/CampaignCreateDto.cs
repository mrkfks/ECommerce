namespace ECommerce.Application.DTOs;

/// <summary>
/// Kampanya oluşturma DTO
/// </summary>
public record CampaignCreateDto
{
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public decimal DiscountPercent { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public int? CompanyId { get; init; }
}
