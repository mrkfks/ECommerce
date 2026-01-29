namespace Dashboard.Web.Models;

public class CategorySalesVm
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public decimal TotalSales { get; set; }
    public int TotalQuantity { get; set; }
    public decimal Percentage { get; set; }
    public string Color { get; set; } = string.Empty;
}
