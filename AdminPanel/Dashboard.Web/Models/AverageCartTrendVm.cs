namespace Dashboard.Web.Models;

public class AverageCartTrendDto
{
    public DateTime Date { get; set; }
    public decimal AverageCartValue { get; set; }
    public int OrderCount { get; set; }
}
