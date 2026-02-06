namespace Dashboard.Web.Models
{
    public class CampaignCreateVm
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal DiscountPercent { get; set; }
        public DateTime StartDate { get; set; } = DateTime.Now;
        public DateTime EndDate { get; set; } = DateTime.Now.AddDays(7);
        public int CompanyId { get; set; }
        public string? BannerImageUrl { get; set; }
    }
}
