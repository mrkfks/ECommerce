using Dashboard.Web.Services;

namespace Dashboard.Web.Models;

/// <summary>
/// Charts sayfası için ViewModel
/// </summary>
public class ChartsViewModel
{
    // Temel KPI Verileri
    public DashboardKpiViewModel? KpiData { get; set; }

    // Şirketler (SuperAdmin için filtre)
    public List<CompanySelectVm> Companies { get; set; } = new();

    // Seçili Filtreler
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int? SelectedCompanyId { get; set; }

    // JSON formatında grafik verileri
    public string SalesTrendJson
    {
        get
        {
            if (KpiData?.RevenueTrend == null || !KpiData.RevenueTrend.Any())
                return "[]";

            return System.Text.Json.JsonSerializer.Serialize(
                KpiData.RevenueTrend.Select(r => new
                {
                    date = r.Date.ToString("dd MMM"),
                    fullDate = r.Date.ToString("yyyy-MM-dd"),
                    revenue = r.Revenue,
                    orders = r.OrderCount
                }));
        }
    }

    public string CategorySalesJson
    {
        get
        {
            if (KpiData?.CategorySales == null || !KpiData.CategorySales.Any())
                return "[]";

            return System.Text.Json.JsonSerializer.Serialize(
                KpiData.CategorySales.Select(c => new
                {
                    categoryId = c.CategoryId,
                    name = c.CategoryName,
                    sales = c.TotalSales,
                    quantity = c.TotalQuantity,
                    percentage = c.Percentage,
                    color = c.Color
                }));
        }
    }

    public string CustomerSegmentJson => System.Text.Json.JsonSerializer.Serialize(new
    {
        newCustomers = KpiData?.CustomerSegmentation?.NewCustomers ?? 0,
        returningCustomers = KpiData?.CustomerSegmentation?.ReturningCustomers ?? 0,
        newRevenue = KpiData?.CustomerSegmentation?.NewCustomersRevenue ?? 0,
        returningRevenue = KpiData?.CustomerSegmentation?.ReturningCustomersRevenue ?? 0,
        newPercent = KpiData?.CustomerSegmentation?.NewCustomerPercent ?? 0,
        returningPercent = KpiData?.CustomerSegmentation?.ReturningCustomerPercent ?? 0
    });

    public string OrderStatusDistributionJson
    {
        get
        {
            if (KpiData?.OrderStatusDistribution == null || !KpiData.OrderStatusDistribution.Any())
                return "[]";

            return System.Text.Json.JsonSerializer.Serialize(
                KpiData.OrderStatusDistribution.Select(d => new
                {
                    date = d.Date.ToString("dd MMM"),
                    pending = d.PendingCount,
                    shipped = d.ShippedCount,
                    delivered = d.DeliveredCount,
                    returned = d.ReturnedCount,
                    cancelled = d.CancelledCount
                }));
        }
    }

    public string GeographicDistributionJson
    {
        get
        {
            if (KpiData?.GeographicDistribution == null || !KpiData.GeographicDistribution.Any())
                return "[]";

            return System.Text.Json.JsonSerializer.Serialize(
                KpiData.GeographicDistribution.Select(g => new
                {
                    city = g.City,
                    state = g.State,
                    orderCount = g.OrderCount,
                    revenue = g.TotalRevenue,
                    percentage = g.Percentage,
                    intensity = g.Intensity
                }));
        }
    }

    public string AverageCartTrendJson
    {
        get
        {
            if (KpiData?.AverageCartTrend == null || !KpiData.AverageCartTrend.Any())
                return "[]";

            return System.Text.Json.JsonSerializer.Serialize(
                KpiData.AverageCartTrend.Select(a => new
                {
                    date = a.Date.ToString("dd MMM"),
                    value = a.AverageCartValue,
                    orders = a.OrderCount
                }));
        }
    }

    public string TopProductsJson
    {
        get
        {
            if (KpiData?.TopProducts == null || !KpiData.TopProducts.Any())
                return "[]";

            return System.Text.Json.JsonSerializer.Serialize(
                KpiData.TopProducts.Select(p => new
                {
                    id = p.ProductId,
                    name = p.ProductName,
                    quantity = p.QuantitySold,
                    revenue = p.Revenue,
                    category = p.CategoryName
                }));
        }
    }
}

/// <summary>
/// Şirket seçim listesi için basit VM
/// </summary>
public class CompanySelectVm
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
