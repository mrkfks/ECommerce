namespace Dashboard.Web.Models;

public class CampaignProductsVm
{
    public int CampaignId { get; set; }
    public string CampaignName { get; set; } = string.Empty;
    public decimal DiscountPercent { get; set; }

    // Categories
    public List<CategorySelectionVm> AllCategories { get; set; } = new();
    public List<int> SelectedCategoryIds { get; set; } = new();

    // Products
    public List<ProductCampaignVm> SelectedProducts { get; set; } = new();
}

public class CategorySelectionVm
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int ProductCount { get; set; }
}
