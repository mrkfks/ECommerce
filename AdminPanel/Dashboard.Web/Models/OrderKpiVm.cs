namespace Dashboard.Web.Models
{
    public class OrderKpiVm
    {
        public int TotalOrders { get; set; }
        public int DailyOrders { get; set; }
        public int DeliveredCount { get; set; }
        public double DeliveredPercent { get; set; }
        public int ShippedCount { get; set; }
        public int CancelledCount { get; set; }
        public int PendingCount { get; set; }
    }
}
