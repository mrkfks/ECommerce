using ECommerce.Application.DTOs.Dashboard;

namespace ECommerce.Application.Interfaces;

public interface IDashboardService
{
    Task<DashboardKpiDto> GetDashboardKpiAsync(DateTime? startDate = null, DateTime? endDate = null, int? companyId = null);
    Task<SalesKpiDto> GetSalesKpiAsync(DateTime? startDate, DateTime? endDate, int? companyId);
    Task<OrderKpiDto> GetOrdersKpiAsync(DateTime? startDate, DateTime? endDate, int? companyId);
    Task<List<TopProductDto>> GetTopProductsAsync(DateTime? startDate, DateTime? endDate, int? companyId);
    Task<List<LowStockProductDto>> GetLowStockProductsAsync(int? companyId);
    Task<List<RevenueTrendDto>> GetRevenueTrendAsync(DateTime? startDate, DateTime? endDate, int? companyId);
    Task<CustomerSegmentationDto> GetCustomerSegmentsAsync(DateTime? startDate, DateTime? endDate, int? companyId);
    Task<List<CategorySalesDto>> GetCategorySalesAsync(DateTime? startDate, DateTime? endDate, int? companyId);
    Task<List<GeographicDistributionDto>> GetGeographicDistributionAsync(DateTime? startDate, DateTime? endDate, int? companyId);
    Task<List<AverageCartTrendDto>> GetAverageCartTrendAsync(DateTime? startDate, DateTime? endDate, int? companyId);
    Task<List<OrderStatusDistributionDto>> GetOrderStatusDistributionAsync(DateTime? startDate, DateTime? endDate, int? companyId);
}
