using System.Net.Http.Json;

namespace Dashboard.Web.Services;

/// <summary>
/// Dashboard KPI API Service - Ana sayfa istatistikleri için
/// </summary>
public class DashboardApiService
{
    private readonly HttpClient _httpClient;

    public DashboardApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    /// Tüm KPI verilerini getirir
    /// </summary>
    public async Task<DashboardKpiVm?> GetKpiAsync(DateTime? startDate = null, DateTime? endDate = null, int? companyId = null)
    {
        try
        {
            var query = new List<string>();
            if (startDate.HasValue) query.Add($"startDate={startDate:yyyy-MM-dd}");
            if (endDate.HasValue) query.Add($"endDate={endDate:yyyy-MM-dd}");
            if (companyId.HasValue) query.Add($"companyId={companyId}");

            var queryString = query.Count > 0 ? "?" + string.Join("&", query) : "";
            return await _httpClient.GetFromJsonAsync<DashboardKpiVm>($"api/Dashboard/kpi{queryString}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DashboardApiService] Error fetching KPI: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Satış KPI'larını getirir
    /// </summary>
    public async Task<SalesKpiVm?> GetSalesKpiAsync(int? companyId = null)
    {
        try
        {
            var query = companyId.HasValue ? $"?companyId={companyId}" : "";
            return await _httpClient.GetFromJsonAsync<SalesKpiVm>($"api/Dashboard/sales{query}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DashboardApiService] Error fetching sales KPI: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// En çok satan ürünleri getirir
    /// </summary>
    public async Task<List<TopProductVm>?> GetTopProductsAsync(int? companyId = null)
    {
        try
        {
            var query = companyId.HasValue ? $"?companyId={companyId}" : "";
            return await _httpClient.GetFromJsonAsync<List<TopProductVm>>($"api/Dashboard/top-products{query}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DashboardApiService] Error fetching top products: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Kritik stok ürünlerini getirir
    /// </summary>
    public async Task<List<LowStockProductVm>?> GetLowStockProductsAsync(int? companyId = null)
    {
        try
        {
            var query = companyId.HasValue ? $"?companyId={companyId}" : "";
            return await _httpClient.GetFromJsonAsync<List<LowStockProductVm>>($"api/Dashboard/low-stock{query}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DashboardApiService] Error fetching low stock products: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Gelir trendi verilerini getirir
    /// </summary>
    public async Task<List<RevenueTrendVm>?> GetRevenueTrendAsync(int? companyId = null)
    {
        try
        {
            var query = companyId.HasValue ? $"?companyId={companyId}" : "";
            return await _httpClient.GetFromJsonAsync<List<RevenueTrendVm>>($"api/Dashboard/revenue-trend{query}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DashboardApiService] Error fetching revenue trend: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Kategori bazlı satış dağılımını getirir
    /// </summary>
    public async Task<List<CategorySalesVm>?> GetCategorySalesAsync(int? companyId = null)
    {
        try
        {
            var query = companyId.HasValue ? $"?companyId={companyId}" : "";
            return await _httpClient.GetFromJsonAsync<List<CategorySalesVm>>($"api/Dashboard/category-sales{query}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DashboardApiService] Error fetching category sales: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Coğrafi dağılım verilerini getirir
    /// </summary>
    public async Task<List<GeographicDistributionVm>?> GetGeographicDistributionAsync(int? companyId = null)
    {
        try
        {
            var query = companyId.HasValue ? $"?companyId={companyId}" : "";
            return await _httpClient.GetFromJsonAsync<List<GeographicDistributionVm>>($"api/Dashboard/geographic-distribution{query}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DashboardApiService] Error fetching geographic distribution: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Ortalama sepet tutarı trendini getirir
    /// </summary>
    public async Task<List<AverageCartTrendVm>?> GetAverageCartTrendAsync(int? companyId = null)
    {
        try
        {
            var query = companyId.HasValue ? $"?companyId={companyId}" : "";
            return await _httpClient.GetFromJsonAsync<List<AverageCartTrendVm>>($"api/Dashboard/average-cart-trend{query}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DashboardApiService] Error fetching average cart trend: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Sipariş durumu dağılımını zaman bazlı getirir
    /// </summary>
    public async Task<List<OrderStatusDistributionVm>?> GetOrderStatusDistributionAsync(int? companyId = null)
    {
        try
        {
            var query = companyId.HasValue ? $"?companyId={companyId}" : "";
            return await _httpClient.GetFromJsonAsync<List<OrderStatusDistributionVm>>($"api/Dashboard/order-status-distribution{query}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DashboardApiService] Error fetching order status distribution: {ex.Message}");
            return null;
        }
    }
}

#region ViewModels

/// <summary>
/// Dashboard KPI ana ViewModel
/// </summary>
public class DashboardKpiVm
{
    public SalesKpiVm Sales { get; set; } = new();
    public OrderKpiVm Orders { get; set; } = new();
    public CustomerKpiVm Customers { get; set; } = new();
    public List<TopProductVm> TopProducts { get; set; } = new();
    public List<LowStockProductVm> LowStockProducts { get; set; } = new();
    public List<RevenueTrendVm> RevenueTrend { get; set; } = new();
    public CustomerSegmentationVm CustomerSegmentation { get; set; } = new();
    public List<CategorySalesVm> CategorySales { get; set; } = new();
    public List<GeographicDistributionVm> GeographicDistribution { get; set; } = new();
    public List<AverageCartTrendVm> AverageCartTrend { get; set; } = new();
    public List<OrderStatusDistributionVm> OrderStatusDistribution { get; set; } = new();
}

public class SalesKpiVm
{
    public decimal DailySales { get; set; }
    public decimal DailySalesChange { get; set; }
    public decimal YesterdaySales { get; set; }
    public decimal WeeklySales { get; set; }
    public decimal WeeklySalesChange { get; set; }
    public decimal LastWeekSales { get; set; }
    public decimal MonthlySales { get; set; }
    public decimal MonthlySalesChange { get; set; }
    public decimal MonthlyTarget { get; set; }
    public decimal AverageOrderValue { get; set; }
    public decimal AverageOrderValueChange { get; set; }

    // Helper properties
    public string DailySalesFormatted => DailySales.ToString("C0", new System.Globalization.CultureInfo("tr-TR"));
    public string WeeklySalesFormatted => WeeklySales.ToString("C0", new System.Globalization.CultureInfo("tr-TR"));
    public string MonthlySalesFormatted => MonthlySales.ToString("C0", new System.Globalization.CultureInfo("tr-TR"));
    public string AverageOrderValueFormatted => AverageOrderValue.ToString("C0", new System.Globalization.CultureInfo("tr-TR"));
    public string YesterdaySalesFormatted => YesterdaySales.ToString("C0", new System.Globalization.CultureInfo("tr-TR"));
    public string LastWeekSalesFormatted => LastWeekSales.ToString("C0", new System.Globalization.CultureInfo("tr-TR"));
    public string MonthlyTargetFormatted => MonthlyTarget.ToString("C0", new System.Globalization.CultureInfo("tr-TR"));
}

public class OrderKpiVm
{
    public int TotalOrders { get; set; }
    public int DailyOrders { get; set; }
    public int PendingCount { get; set; }
    public int ShippedCount { get; set; }
    public int DeliveredCount { get; set; }
    public int ReturnedCount { get; set; }
    public int CancelledCount { get; set; }
    public decimal PendingPercent { get; set; }
    public decimal ShippedPercent { get; set; }
    public decimal DeliveredPercent { get; set; }
    public decimal ReturnedPercent { get; set; }
    public decimal CancelledPercent { get; set; }
}

public class CustomerKpiVm
{
    public int TotalCustomers { get; set; }
    public int DailyNewCustomers { get; set; }
    public int MonthlyNewCustomers { get; set; }
    public decimal CustomerGrowthRate { get; set; }
}

public class TopProductVm
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public int QuantitySold { get; set; }
    public decimal Revenue { get; set; }
    public string CategoryName { get; set; } = string.Empty;

    public string RevenueFormatted => Revenue.ToString("C0", new System.Globalization.CultureInfo("tr-TR"));
}

public class LowStockProductVm
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public int CurrentStock { get; set; }
    public decimal DailyAverageSales { get; set; }
    public int DaysUntilOutOfStock { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public bool IsCritical => CurrentStock < 5;
}

public class RevenueTrendVm
{
    public DateTime Date { get; set; }
    public decimal Revenue { get; set; }
    public int OrderCount { get; set; }
}

public class CustomerSegmentationVm
{
    public int NewCustomers { get; set; }
    public int ReturningCustomers { get; set; }
    public decimal NewCustomersRevenue { get; set; }
    public decimal ReturningCustomersRevenue { get; set; }
    public decimal NewCustomerPercent { get; set; }
    public decimal ReturningCustomerPercent { get; set; }
}

/// <summary>
/// Kategori bazlı satış dağılımı (Pie Chart için)
/// </summary>
public class CategorySalesVm
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public decimal TotalSales { get; set; }
    public int TotalQuantity { get; set; }
    public decimal Percentage { get; set; }
    public string Color { get; set; } = string.Empty;

    public string TotalSalesFormatted => TotalSales.ToString("C0", new System.Globalization.CultureInfo("tr-TR"));
}

/// <summary>
/// Coğrafi dağılım (Heatmap için)
/// </summary>
public class GeographicDistributionVm
{
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public int OrderCount { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal Percentage { get; set; }
    public string Intensity { get; set; } = "low";

    public string TotalRevenueFormatted => TotalRevenue.ToString("C0", new System.Globalization.CultureInfo("tr-TR"));

    public string IntensityClass => Intensity switch
    {
        "critical" => "bg-danger",
        "high" => "bg-warning",
        "medium" => "bg-info",
        _ => "bg-success"
    };
}

/// <summary>
/// Ortalama sepet tutarı trendi (Line Chart için)
/// </summary>
public class AverageCartTrendVm
{
    public DateTime Date { get; set; }
    public decimal AverageCartValue { get; set; }
    public int OrderCount { get; set; }

    public string AverageCartValueFormatted => AverageCartValue.ToString("C0", new System.Globalization.CultureInfo("tr-TR"));
}

/// <summary>
/// Sipariş durumu zaman bazlı dağılımı (Stacked Bar Chart için)
/// </summary>
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

#endregion
