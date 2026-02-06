namespace Dashboard.Web.Models;

public class ProductCampaignCreateVm
{
    public int ProductId { get; set; }
    public int CampaignId { get; set; }
    public decimal OriginalPrice { get; set; }
    public decimal DiscountedPrice { get; set; }
}
