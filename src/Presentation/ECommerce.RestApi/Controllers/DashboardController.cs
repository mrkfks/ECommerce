using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.RestApi.Controllers;

/// <summary>
/// Dashboard ve KPI verilerini sağlayan controller
/// </summary>
[ApiController]
[Route("api/dashboard")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(IDashboardService dashboardService, ILogger<DashboardController> logger)
    {
        _dashboardService = dashboardService;
        _logger = logger;
    }

    /// <summary>
    /// Dashboard KPI verilerini getirir
    /// </summary>
    /// <param name="startDate">Başlangıç tarihi (opsiyonel)</param>
    /// <param name="endDate">Bitiş tarihi (opsiyonel)</param>
    /// <param name="companyId">Şirket ID (sadece SuperAdmin için)</param>
    /// <returns>Dashboard KPI verileri</returns>
    [HttpGet("kpi")]
    [ProducesResponseType(typeof(DashboardKpiDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<DashboardKpiDto>> GetKpi(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] int? companyId = null)
    {
        _logger.LogInformation("Fetching dashboard KPI data. StartDate: {StartDate}, EndDate: {EndDate}, CompanyId: {CompanyId}",
            startDate, endDate, companyId);

        var result = await _dashboardService.GetDashboardKpiAsync(startDate, endDate, companyId);
        return Ok(result);
    }

    /// <summary>
    /// Sadece satış KPI'larını getirir
    /// </summary>
    [HttpGet("sales")]
    [ProducesResponseType(typeof(SalesKpiDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<SalesKpiDto>> GetSalesKpi([FromQuery] int? companyId = null)
    {
        var result = await _dashboardService.GetSalesKpiAsync(null, null, companyId);
        return Ok(result);
    }

    /// <summary>
    /// Sadece sipariş KPI'larını getirir
    /// </summary>
    [HttpGet("orders")]
    [ProducesResponseType(typeof(OrderKpiDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<OrderKpiDto>> GetOrdersKpi([FromQuery] int? companyId = null)
    {
        var result = await _dashboardService.GetOrdersKpiAsync(null, null, companyId);
        return Ok(result);
    }

    /// <summary>
    /// En çok satan ürünleri getirir
    /// </summary>
    [HttpGet("top-products")]
    [ProducesResponseType(typeof(List<TopProductDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<TopProductDto>>> GetTopProducts([FromQuery] int? companyId = null)
    {
        var result = await _dashboardService.GetTopProductsAsync(null, null, companyId);
        return Ok(result);
    }

    /// <summary>
    /// Kritik stok seviyesindeki ürünleri getirir
    /// </summary>
    [HttpGet("low-stock")]
    [ProducesResponseType(typeof(List<LowStockProductDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<LowStockProductDto>>> GetLowStockProducts([FromQuery] int? companyId = null)
    {
        var result = await _dashboardService.GetLowStockProductsAsync(companyId);
        return Ok(result);
    }

    /// <summary>
    /// Gelir trendi verilerini getirir (son 30 gün)
    /// </summary>
    [HttpGet("revenue-trend")]
    [ProducesResponseType(typeof(List<RevenueTrendDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<RevenueTrendDto>>> GetRevenueTrend([FromQuery] int? companyId = null)
    {
        var result = await _dashboardService.GetRevenueTrendAsync(null, null, companyId);
        return Ok(result);
    }

    /// <summary>
    /// Müşteri segmentasyonu verilerini getirir
    /// </summary>
    [HttpGet("customer-segments")]
    [ProducesResponseType(typeof(CustomerSegmentationDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<CustomerSegmentationDto>> GetCustomerSegments([FromQuery] int? companyId = null)
    {
        var result = await _dashboardService.GetCustomerSegmentsAsync(null, null, companyId);
        return Ok(result);
    }

    /// <summary>
    /// Kategori bazlı satış dağılımını getirir (Pie Chart için)
    /// </summary>
    [HttpGet("category-sales")]
    [ProducesResponseType(typeof(List<CategorySalesDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<CategorySalesDto>>> GetCategorySales([FromQuery] int? companyId = null)
    {
        var result = await _dashboardService.GetCategorySalesAsync(null, null, companyId);
        return Ok(result);
    }

    /// <summary>
    /// Kategori bazlı stok dağılımını getirir (Pie Chart için)
    /// </summary>
    [HttpGet("category-stock")]
    [ProducesResponseType(typeof(List<CategoryStockDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<CategoryStockDto>>> GetCategoryStockDistribution([FromQuery] int? companyId = null)
    {
        var result = await _dashboardService.GetCategoryStockDistributionAsync(companyId);
        return Ok(result);
    }

    /// <summary>
    /// Coğrafi dağılım verilerini getirir (Heatmap için)
    /// </summary>
    [HttpGet("geographic-distribution")]
    [ProducesResponseType(typeof(List<GeographicDistributionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<GeographicDistributionDto>>> GetGeographicDistribution([FromQuery] int? companyId = null)
    {
        var result = await _dashboardService.GetGeographicDistributionAsync(null, null, companyId);
        return Ok(result);
    }

    /// <summary>
    /// Ortalama sepet tutarı trendini getirir (Line Chart için)
    /// </summary>
    [HttpGet("average-cart-trend")]
    [ProducesResponseType(typeof(List<AverageCartTrendDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<AverageCartTrendDto>>> GetAverageCartTrend([FromQuery] int? companyId = null)
    {
        var result = await _dashboardService.GetAverageCartTrendAsync(null, null, companyId);
        return Ok(result);
    }

    /// <summary>
    /// Sipariş durumu dağılımını zaman bazlı getirir (Stacked Bar Chart için)
    /// </summary>
    [HttpGet("order-status-distribution")]
    [ProducesResponseType(typeof(List<OrderStatusDistributionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<OrderStatusDistributionDto>>> GetOrderStatusDistribution([FromQuery] int? companyId = null)
    {
        var result = await _dashboardService.GetOrderStatusDistributionAsync(null, null, companyId);
        return Ok(result);
    }
}
