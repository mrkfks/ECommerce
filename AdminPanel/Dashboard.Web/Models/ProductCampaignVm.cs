namespace Dashboard.Web.Models;

public class ProductCampaignVm
{
    public int ProductId { get; set; }
    public int CampaignId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string CampaignName { get; set; } = string.Empty;
    public decimal OriginalPrice { get; set; }
    public decimal DiscountedPrice { get; set; }
    public decimal DiscountPercentage { get; set; }
    public decimal SavedAmount { get; set; }
    public DateTime CampaignStartDate { get; set; }
    public DateTime CampaignEndDate { get; set; }
    public bool IsCampaignActive { get; set; }
    public DateTime CreatedAt { get; set; }

    // UI Helpers
    public string PriceDisplayText => $"₺{OriginalPrice:F2} → ₺{DiscountedPrice:F2}";
    public string DiscountBadgeText => $"-%{DiscountPercentage:F0}";
    public string SavedText => $"₺{SavedAmount:F2} Tasarruf";
}
