namespace Dashboard.Web.Models;

public class CampaignVm
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
    public string? BannerImageUrl { get; set; }

    // UI Helpers
    public string DiscountText => $"%{DiscountPercent:N0}";
    public string DateRangeText => $"{StartDate:dd MMM} - {EndDate:dd MMM yyyy}";
    public string StatusBadgeClass => IsCurrentlyActive ? "bg-success" : (StartDate > DateTime.Now ? "bg-info" : "bg-secondary");
    public string StatusText => IsCurrentlyActive ? "Aktif" : (StartDate > DateTime.Now ? "Yaklaşan" : "Sona Erdi");
    public string RemainingDaysText => RemainingDays > 0 ? $"{RemainingDays} gün kaldı" : "Sona erdi";
}
