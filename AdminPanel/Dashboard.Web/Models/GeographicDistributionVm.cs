namespace Dashboard.Web.Models;

public class GeographicDistributionVm
{
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public int OrderCount { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal Percentage { get; set; }
    public string Intensity { get; set; } = "low";

    public string TotalRevenueFormatted => TotalRevenue.ToString("C0", new System.Globalization.CultureInfo("tr-TR"));

    public string IntensityClass => Intensity switch
    {
        "critical" => "bg-danger",
        "high" => "bg-warning",
        "medium" => "bg-info",
        _ => "bg-success"
    };
}
