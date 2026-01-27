namespace Dashboard.Web.Models
{
    public class LowStockItemVm
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public int CurrentStock { get; set; }
        public int Threshold { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string BrandName { get; set; } = string.Empty;
        public decimal Price { get; set; }

        public string StockStatusColor => CurrentStock == 0 ? "danger" : CurrentStock < 5 ? "warning" : "info";
        public string StockStatusText => CurrentStock == 0 ? "Stokta Yok" : CurrentStock < 5 ? "Kritik" : "Düşük";
    }
}
