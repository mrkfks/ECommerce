namespace Dashboard.Web.Models;

public class OrderStatusDistributionVm
{
    public DateTime Date { get; set; }
    public int PendingCount { get; set; }
    public int ShippedCount { get; set; }
    public int DeliveredCount { get; set; }
    public int ReturnedCount { get; set; }
    public int CancelledCount { get; set; }

    public int TotalCount => PendingCount + ShippedCount + DeliveredCount + ReturnedCount + CancelledCount;
}
