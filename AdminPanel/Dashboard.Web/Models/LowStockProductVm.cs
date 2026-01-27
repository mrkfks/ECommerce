namespace Dashboard.Web.Models
{
    public class LowStockProductVm
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int StockQuantity { get; set; }
        public int Threshold { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string BrandName { get; set; } = string.Empty;
        public int DaysUntilOutOfStock { get; set; }
        public int CurrentStock { get; set; }
        public int DailyAverageSales { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public bool IsCritical { get; set; }
    }
}
