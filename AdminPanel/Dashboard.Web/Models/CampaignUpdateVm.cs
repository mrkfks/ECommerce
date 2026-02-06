namespace Dashboard.Web.Models;

public class CampaignUpdateVm
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal DiscountPercent { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? BannerImageUrl { get; set; }
}
