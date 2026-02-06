namespace ECommerce.Application.DTOs;

/// <summary>
/// Ürün-Kampanya ilişkisi DTO
/// </summary>
public record ProductCampaignDto
{
    public int ProductId { get; init; }
    public int CampaignId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public string CampaignName { get; init; } = string.Empty;
    public decimal OriginalPrice { get; init; }
    public decimal DiscountedPrice { get; init; }
    public decimal DiscountPercentage { get; init; }
    public decimal SavingAmount => OriginalPrice - DiscountedPrice;
    public DateTime CampaignStartDate { get; init; }
    public DateTime CampaignEndDate { get; init; }
    public bool IsCampaignActive { get; init; }
    public DateTime CreatedAt { get; init; }
}

/// <summary>
/// Ürün-Kampanya form DTO
/// </summary>
public record ProductCampaignFormDto
{
    public int ProductId { get; init; }
    public int CampaignId { get; init; }
    public decimal OriginalPrice { get; init; }
    public decimal DiscountedPrice { get; init; }
}
