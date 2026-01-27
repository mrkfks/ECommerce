

namespace Dashboard.Web.Models;

/// <summary>
/// Dashboard KPI View Model - Ana sayfa için
/// </summary>
public class DashboardKpiViewModel
{
    public SalesKpiVm Sales { get; set; } = new();
    public OrderKpiVm Orders { get; set; } = new();
    public CustomerKpiVm Customers { get; set; } = new();
    public List<TopProductVm> TopProducts { get; set; } = new();
    public List<LowStockProductVm> LowStockProducts { get; set; } = new();
    public List<RevenueTrendVm> RevenueTrend { get; set; } = new();
    public CustomerSegmentationVm CustomerSegmentation { get; set; } = new();

    // Helper properties for view
    public string RevenueTrendJson => System.Text.Json.JsonSerializer.Serialize(
        RevenueTrend.Select(r => new { date = r.Date.ToString("dd MMM"), revenue = r.Revenue, orders = r.OrderCount }));

    public string OrderStatusJson => System.Text.Json.JsonSerializer.Serialize(new
    {
        labels = new[] { "Beklemede", "Kargoda", "Teslim Edildi", "İptal" },
        data = new[] { Orders.PendingCount, Orders.ShippedCount, Orders.DeliveredCount, Orders.CancelledCount }
    });

    public string CustomerSegmentJson => System.Text.Json.JsonSerializer.Serialize(new
    {
        labels = new[] { "Yeni Müşteri", "Tekrar Müşteri" },
        data = new[] { CustomerSegmentation.NewCustomers, CustomerSegmentation.ReturningCustomers },
        revenue = new[] { CustomerSegmentation.NewCustomersRevenue, CustomerSegmentation.ReturningCustomersRevenue }
    });
}
