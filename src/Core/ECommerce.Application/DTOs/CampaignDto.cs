namespace ECommerce.Application.DTOs
{
    public class CampaignDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal DiscountPercent { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
        public bool IsCurrentlyActive { get; set; }
        public int RemainingDays { get; set; }
        public int CompanyId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class CampaignCreateDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal DiscountPercent { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int CompanyId { get; set; }
    }

    public class CampaignUpdateDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal DiscountPercent { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class CampaignSummaryDto
    {
        public int TotalCampaigns { get; set; }
        public int ActiveCampaigns { get; set; }
        public int UpcomingCampaigns { get; set; }
        public int ExpiredCampaigns { get; set; }
        public List<CampaignDto> CurrentCampaigns { get; set; } = new();
    }
}
