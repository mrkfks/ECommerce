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

            // API'den yanıt al
            var jsonResponse = await _httpClient.GetStringAsync($"api/dashboard/kpi{queryString}");

            // JSON yanıtı parse et
            using (var jsonDoc = System.Text.Json.JsonDocument.Parse(jsonResponse))
            {
                var data = jsonDoc.RootElement.GetProperty("data");

                // DashboardKpiViewModel'e dönüştür
                var viewModel = new DashboardKpiViewModel
                {
                    Sales = MapSalesKpi(data),
                    Orders = MapOrdersKpi(data),
                    Customers = MapCustomersKpi(data),
                    Products = MapProductsKpi(data),
                    TopProducts = MapTopProducts(data),
                    LowStockProducts = MapLowStockProducts(data),
                    RevenueTrend = MapRevenueTrend(data),
                    CustomerSegmentation = MapCustomerSegmentation(data),
                    CategorySales = MapCategorySales(data),
                    CategoryStock = MapCategoryStock(data),
                    GeographicDistribution = MapGeographicDistribution(data),
                    AverageCartTrend = MapAverageCartTrend(data),
                    OrderStatusDistribution = MapOrderStatusDistribution(data)
                };

                return viewModel;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DashboardApiService] Error fetching KPI: {ex.Message}");
            return null;
        }
    }

    private SalesKpiVm MapSalesKpi(System.Text.Json.JsonElement data)
    {
        if (data.TryGetProperty("sales", out var sales))
        {
            var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            return System.Text.Json.JsonSerializer.Deserialize<SalesKpiVm>(sales.GetRawText(), options) ?? new();
        }
        return new();
    }

    private OrderKpiVm MapOrdersKpi(System.Text.Json.JsonElement data)
    {
        if (data.TryGetProperty("orders", out var orders))
        {
            var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            return System.Text.Json.JsonSerializer.Deserialize<OrderKpiVm>(orders.GetRawText(), options) ?? new();
        }
        return new();
    }

    private CustomerKpiVm MapCustomersKpi(System.Text.Json.JsonElement data)
    {
        if (data.TryGetProperty("customers", out var customers))
        {
            var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            return System.Text.Json.JsonSerializer.Deserialize<CustomerKpiVm>(customers.GetRawText(), options) ?? new();
        }
        return new();
    }

    private ProductKpiVm MapProductsKpi(System.Text.Json.JsonElement data)
    {
        if (data.TryGetProperty("products", out var products))
        {
            var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            return System.Text.Json.JsonSerializer.Deserialize<ProductKpiVm>(products.GetRawText(), options) ?? new();
        }
        return new();
    }

    private List<Dashboard.Web.Models.TopProductDto> MapTopProducts(System.Text.Json.JsonElement data)
    {
        if (!data.TryGetProperty("topProducts", out var topProducts)) return new();
        var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        return System.Text.Json.JsonSerializer.Deserialize<List<Dashboard.Web.Models.TopProductDto>>(topProducts.GetRawText(), options) ?? new();
    }

    private List<LowStockProductVm> MapLowStockProducts(System.Text.Json.JsonElement data)
    {
        if (!data.TryGetProperty("lowStockProducts", out var lowStock)) return new();
        var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        return System.Text.Json.JsonSerializer.Deserialize<List<LowStockProductVm>>(lowStock.GetRawText(), options) ?? new();
    }

    private List<Dashboard.Web.Models.RevenueTrendDto> MapRevenueTrend(System.Text.Json.JsonElement data)
    {
        if (!data.TryGetProperty("revenueTrend", out var trend)) return new();
        var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        return System.Text.Json.JsonSerializer.Deserialize<List<Dashboard.Web.Models.RevenueTrendDto>>(trend.GetRawText(), options) ?? new();
    }

    private CustomerSegmentationVm MapCustomerSegmentation(System.Text.Json.JsonElement data)
    {
        if (!data.TryGetProperty("customerSegmentation", out var seg))
            return new CustomerSegmentationVm { NewCustomers = 0, ReturningCustomers = 0 };
        var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        return System.Text.Json.JsonSerializer.Deserialize<CustomerSegmentationVm>(seg.GetRawText(), options) ?? new();
    }

    private List<CategorySalesVm> MapCategorySales(System.Text.Json.JsonElement data)
    {
        if (!data.TryGetProperty("categorySales", out var cat)) return new();
        var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        return System.Text.Json.JsonSerializer.Deserialize<List<CategorySalesVm>>(cat.GetRawText(), options) ?? new();
    }

    private List<Dashboard.Web.Models.CategoryStockDto> MapCategoryStock(System.Text.Json.JsonElement data)
    {
        if (!data.TryGetProperty("categoryStock", out var cat)) return new();
        var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        return System.Text.Json.JsonSerializer.Deserialize<List<Dashboard.Web.Models.CategoryStockDto>>(cat.GetRawText(), options) ?? new();
    }

    private List<Dashboard.Web.Models.GeographicDistributionDto> MapGeographicDistribution(System.Text.Json.JsonElement data)
    {
        if (!data.TryGetProperty("geographicDistribution", out var geo)) return new();
        var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        return System.Text.Json.JsonSerializer.Deserialize<List<Dashboard.Web.Models.GeographicDistributionDto>>(geo.GetRawText(), options) ?? new();
    }

    private List<Dashboard.Web.Models.AverageCartTrendDto> MapAverageCartTrend(System.Text.Json.JsonElement data)
    {
        if (!data.TryGetProperty("averageCartTrend", out var cart)) return new();
        var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        return System.Text.Json.JsonSerializer.Deserialize<List<Dashboard.Web.Models.AverageCartTrendDto>>(cart.GetRawText(), options) ?? new();
    }

    private List<Dashboard.Web.Models.OrderStatusDistributionDto> MapOrderStatusDistribution(System.Text.Json.JsonElement data)
    {
        if (!data.TryGetProperty("orderStatusDistribution", out var status)) return new();
        var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        return System.Text.Json.JsonSerializer.Deserialize<List<Dashboard.Web.Models.OrderStatusDistributionDto>>(status.GetRawText(), options) ?? new();
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
