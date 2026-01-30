using System.Net.Http.Json;
using Dashboard.Web.Models;

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
    public async Task<DashboardKpiViewModel?> GetKpiAsync(DateTime? startDate = null, DateTime? endDate = null, int? companyId = null)
    {
        try
        {
            var query = new List<string>();
            if (startDate.HasValue) query.Add($"startDate={startDate:yyyy-MM-dd}");
            if (endDate.HasValue) query.Add($"endDate={endDate:yyyy-MM-dd}");
            if (companyId.HasValue) query.Add($"companyId={companyId}");

            var queryString = query.Count > 0 ? "?" + string.Join("&", query) : "";
            return await _httpClient.GetFromJsonAsync<DashboardKpiViewModel>($"api/dashboard/kpi{queryString}");
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
            return await _httpClient.GetFromJsonAsync<SalesKpiVm>($"api/dashboard/sales{query}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DashboardApiService] Error fetching sales KPI: {ex.Message}");
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
            return await _httpClient.GetFromJsonAsync<List<LowStockProductVm>>($"api/dashboard/low-stock{query}");
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
    public async Task<List<RevenueTrendDto>?> GetRevenueTrendAsync(int? companyId = null)
    {
        try
        {
            var query = companyId.HasValue ? $"?companyId={companyId}" : "";
            return await _httpClient.GetFromJsonAsync<List<RevenueTrendDto>>($"api/dashboard/revenue-trend{query}");
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
    public async Task<List<CategorySalesVm>?> GetCategorySalesAsync(DateTime? startDate = null, DateTime? endDate = null, int? companyId = null)
    {
        try
        {
            var queryParams = new List<string>();
            if (startDate.HasValue) queryParams.Add($"startDate={startDate:yyyy-MM-dd}");
            if (endDate.HasValue) queryParams.Add($"endDate={endDate:yyyy-MM-dd}");
            if (companyId.HasValue) queryParams.Add($"companyId={companyId}");
            var query = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
            return await _httpClient.GetFromJsonAsync<List<CategorySalesVm>>($"api/dashboard/category-sales{query}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DashboardApiService] Error fetching category sales: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Kategori bazlı stok dağılımını getirir
    /// </summary>
    public async Task<List<CategoryStockDto>?> GetCategoryStockAsync(DateTime? startDate = null, DateTime? endDate = null, int? companyId = null)
    {
        try
        {
            var queryParams = new List<string>();
            if (startDate.HasValue) queryParams.Add($"startDate={startDate:yyyy-MM-dd}");
            if (endDate.HasValue) queryParams.Add($"endDate={endDate:yyyy-MM-dd}");
            if (companyId.HasValue) queryParams.Add($"companyId={companyId}");
            var query = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
            return await _httpClient.GetFromJsonAsync<List<CategoryStockDto>>($"api/dashboard/category-stock{query}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DashboardApiService] Error fetching category stock: {ex.Message}");
            return null;
        }
    }
    public async Task<List<GeographicDistributionDto>?> GetGeographicDistributionAsync(DateTime? startDate = null, DateTime? endDate = null, int? companyId = null)
    {
        try
        {
            var queryParams = new List<string>();
            if (startDate.HasValue) queryParams.Add($"startDate={startDate:yyyy-MM-dd}");
            if (endDate.HasValue) queryParams.Add($"endDate={endDate:yyyy-MM-dd}");
            if (companyId.HasValue) queryParams.Add($"companyId={companyId}");
            var query = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
            return await _httpClient.GetFromJsonAsync<List<GeographicDistributionDto>>($"api/dashboard/geographic-distribution{query}");
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
    public async Task<List<AverageCartTrendDto>?> GetAverageCartTrendAsync(DateTime? startDate = null, DateTime? endDate = null, int? companyId = null)
    {
        try
        {
            var queryParams = new List<string>();
            if (startDate.HasValue) queryParams.Add($"startDate={startDate:yyyy-MM-dd}");
            if (endDate.HasValue) queryParams.Add($"endDate={endDate:yyyy-MM-dd}");
            if (companyId.HasValue) queryParams.Add($"companyId={companyId}");
            var query = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
            return await _httpClient.GetFromJsonAsync<List<AverageCartTrendDto>>($"api/dashboard/average-cart-trend{query}");
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
    public async Task<List<OrderStatusDistributionDto>?> GetOrderStatusDistributionAsync(DateTime? startDate = null, DateTime? endDate = null, int? companyId = null)
    {
        try
        {
            var queryParams = new List<string>();
            if (startDate.HasValue) queryParams.Add($"startDate={startDate:yyyy-MM-dd}");
            if (endDate.HasValue) queryParams.Add($"endDate={endDate:yyyy-MM-dd}");
            if (companyId.HasValue) queryParams.Add($"companyId={companyId}");
            var query = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
            return await _httpClient.GetFromJsonAsync<List<OrderStatusDistributionDto>>($"api/dashboard/order-status-distribution{query}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DashboardApiService] Error fetching order status distribution: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// En çok satan ürünleri getirir
    /// </summary>
    public async Task<List<TopProductDto>?> GetTopProductsAsync(DateTime? startDate = null, DateTime? endDate = null, int? companyId = null)
    {
        try
        {
            var queryParams = new List<string>();
            if (startDate.HasValue) queryParams.Add($"startDate={startDate:yyyy-MM-dd}");
            if (endDate.HasValue) queryParams.Add($"endDate={endDate:yyyy-MM-dd}");
            if (companyId.HasValue) queryParams.Add($"companyId={companyId}");
            var query = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
            return await _httpClient.GetFromJsonAsync<List<TopProductDto>>($"api/dashboard/top-products{query}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DashboardApiService] Error fetching top products: {ex.Message}");
            return null;
        }
    }
}
