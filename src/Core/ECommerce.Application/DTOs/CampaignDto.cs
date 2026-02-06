namespace ECommerce.Application.DTOs;

/// <summary>
/// Kampanya bilgisi DTO
/// </summary>
public record CampaignDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? BannerImageUrl { get; init; }
    public decimal DiscountPercent { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public bool IsActive { get; init; }
    public bool IsCurrentlyActive { get; init; }
    public int RemainingDays { get; init; }
    public int CompanyId { get; init; }
    public string CompanyName { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}

/// <summary>
/// Kampanya form DTO
/// </summary>
public record CampaignFormDto
{
    public int? Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? BannerImageUrl { get; init; }
    public decimal DiscountPercent { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public int? CompanyId { get; init; }
}

/// <summary>
/// Kampanya özet DTO
/// </summary>
public record CampaignSummaryDto
{
    public int TotalCampaigns { get; init; }
    public int ActiveCampaigns { get; init; }
    public int UpcomingCampaigns { get; init; }
    public int ExpiredCampaigns { get; init; }
    public List<CampaignDto> CurrentCampaigns { get; init; } = new();
}
