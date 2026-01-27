namespace ECommerce.Application.DTOs;

public record CampaignDto(
    int Id,
    string Name,
    string? Description,
    decimal DiscountPercent,
    DateTime StartDate,
    DateTime EndDate,
    bool IsActive,
    bool IsCurrentlyActive,
    int RemainingDays,
    int CompanyId,
    string CompanyName,
    DateTime CreatedAt
);

public record CampaignFormDto(
    int? Id,
    string Name,
    string? Description,
    decimal DiscountPercent,
    DateTime StartDate,
    DateTime EndDate,
    int? CompanyId = null
);

public record CampaignSummaryDto(
    int TotalCampaigns,
    int ActiveCampaigns,
    int UpcomingCampaigns,
    int ExpiredCampaigns,
    List<CampaignDto> CurrentCampaigns
);
