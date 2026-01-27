namespace Dashboard.Web.Models;

public class AverageCartTrendVm
{
    public DateTime Date { get; set; }
    public decimal AverageCartValue { get; set; }
    public int OrderCount { get; set; }

    public string AverageCartValueFormatted => AverageCartValue.ToString("C0", new System.Globalization.CultureInfo("tr-TR"));
}
