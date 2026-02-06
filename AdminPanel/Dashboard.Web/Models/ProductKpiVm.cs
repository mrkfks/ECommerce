namespace Dashboard.Web.Models
{
    public class ProductKpiVm
    {
        public int TotalProducts { get; set; }
        public int ActiveProducts { get; set; }
        public int InactiveProducts { get; set; }
        public int LowStockCount { get; set; }
        public int OutOfStockCount { get; set; }
    }
}
