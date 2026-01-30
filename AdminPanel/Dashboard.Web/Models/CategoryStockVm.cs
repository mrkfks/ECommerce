namespace Dashboard.Web.Models;

public class CategoryStockDto
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int StockQuantity { get; set; }
    public decimal Percentage { get; set; }
    public string Color { get; set; } = string.Empty;
}
