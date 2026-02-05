using System.Net.Http.Json;
using Dashboard.Web.Models;
using ECommerce.Application.DTOs;
using ECommerce.Application.Responses;

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
            var response = await _httpClient.GetFromJsonAsync<ApiResponse<DashboardKpiViewModel>>($"api/dashboard/kpi{queryString}");
            return response?.Data;
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
            var response = await _httpClient.GetFromJsonAsync<ApiResponse<SalesKpiVm>>($"api/dashboard/sales{query}");
            return response?.Data;
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
            var response = await _httpClient.GetFromJsonAsync<ApiResponse<List<LowStockProductVm>>>($"api/dashboard/low-stock{query}");
            return response?.Data;
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
    public async Task<List<ECommerce.Application.DTOs.RevenueTrendDto>?> GetRevenueTrendAsync(int? companyId = null)
    {
        try
        {
            var query = companyId.HasValue ? $"?companyId={companyId}" : "";
            var response = await _httpClient.GetFromJsonAsync<ApiResponse<List<ECommerce.Application.DTOs.RevenueTrendDto>>>($"api/dashboard/revenue-trend{query}");
            return response?.Data;
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
            var response = await _httpClient.GetFromJsonAsync<ApiResponse<List<CategorySalesVm>>>($"api/dashboard/category-sales{query}");
            return response?.Data;
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
    public async Task<List<ECommerce.Application.DTOs.CategoryStockDto>?> GetCategoryStockAsync(DateTime? startDate = null, DateTime? endDate = null, int? companyId = null)
    {
        try
        {
            var queryParams = new List<string>();
            if (startDate.HasValue) queryParams.Add($"startDate={startDate:yyyy-MM-dd}");
            if (endDate.HasValue) queryParams.Add($"endDate={endDate:yyyy-MM-dd}");
            if (companyId.HasValue) queryParams.Add($"companyId={companyId}");
            var query = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
            var response = await _httpClient.GetFromJsonAsync<ApiResponse<List<ECommerce.Application.DTOs.CategoryStockDto>>>($"api/dashboard/category-stock{query}");
            return response?.Data;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DashboardApiService] Error fetching category stock: {ex.Message}");
            return null;
        }
    }

    public async Task<List<ECommerce.Application.DTOs.GeographicDistributionDto>?> GetGeographicDistributionAsync(DateTime? startDate = null, DateTime? endDate = null, int? companyId = null)
    {
        try
        {
            var queryParams = new List<string>();
            if (startDate.HasValue) queryParams.Add($"startDate={startDate:yyyy-MM-dd}");
            if (endDate.HasValue) queryParams.Add($"endDate={endDate:yyyy-MM-dd}");
            if (companyId.HasValue) queryParams.Add($"companyId={companyId}");
            var query = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
            var response = await _httpClient.GetFromJsonAsync<ApiResponse<List<ECommerce.Application.DTOs.GeographicDistributionDto>>>($"api/dashboard/geographic-distribution{query}");
            return response?.Data;
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
    public async Task<List<ECommerce.Application.DTOs.AverageCartTrendDto>?> GetAverageCartTrendAsync(DateTime? startDate = null, DateTime? endDate = null, int? companyId = null)
    {
        try
        {
            var queryParams = new List<string>();
            if (startDate.HasValue) queryParams.Add($"startDate={startDate:yyyy-MM-dd}");
            if (endDate.HasValue) queryParams.Add($"endDate={endDate:yyyy-MM-dd}");
            if (companyId.HasValue) queryParams.Add($"companyId={companyId}");
            var query = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
            var response = await _httpClient.GetFromJsonAsync<ApiResponse<List<ECommerce.Application.DTOs.AverageCartTrendDto>>>($"api/dashboard/average-cart-trend{query}");
            return response?.Data;
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
    public async Task<List<ECommerce.Application.DTOs.OrderStatusDistributionDto>?> GetOrderStatusDistributionAsync(DateTime? startDate = null, DateTime? endDate = null, int? companyId = null)
    {
        try
        {
            var queryParams = new List<string>();
            if (startDate.HasValue) queryParams.Add($"startDate={startDate:yyyy-MM-dd}");
            if (endDate.HasValue) queryParams.Add($"endDate={endDate:yyyy-MM-dd}");
            if (companyId.HasValue) queryParams.Add($"companyId={companyId}");
            var query = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
            var response = await _httpClient.GetFromJsonAsync<ApiResponse<List<ECommerce.Application.DTOs.OrderStatusDistributionDto>>>($"api/dashboard/order-status-distribution{query}");
            return response?.Data;
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
    public async Task<List<ECommerce.Application.DTOs.TopProductDto>?> GetTopProductsAsync(DateTime? startDate = null, DateTime? endDate = null, int? companyId = null)
    {
        try
        {
            var queryParams = new List<string>();
            if (startDate.HasValue) queryParams.Add($"startDate={startDate:yyyy-MM-dd}");
            if (endDate.HasValue) queryParams.Add($"endDate={endDate:yyyy-MM-dd}");
            if (companyId.HasValue) queryParams.Add($"companyId={companyId}");
            var query = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
            var response = await _httpClient.GetFromJsonAsync<ApiResponse<List<ECommerce.Application.DTOs.TopProductDto>>>($"api/dashboard/top-products{query}");
            return response?.Data;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DashboardApiService] Error fetching top products: {ex.Message}");
            return null;
        }
    }
}
