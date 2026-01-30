namespace Dashboard.Web.Models;

public class GeographicDistributionDto
{
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public int OrderCount { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal Percentage { get; set; }
    public string Intensity { get; set; } = "low";
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}
