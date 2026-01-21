using ECommerce.Application.DTOs.Dashboard;
using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ECommerce.Infrastructure.Services;

public class DashboardService : IDashboardService
{
    private readonly AppDbContext _context;
    private readonly ITenantService _tenantService;
    private readonly ILogger<DashboardService> _logger;

    public DashboardService(AppDbContext context, ITenantService tenantService, ILogger<DashboardService> logger)
    {
        _context = context;
        _tenantService = tenantService;
        _logger = logger;
    }

    private IQueryable<Order> FilterOrders(IQueryable<Order> query, DateTime? startDate, DateTime? endDate, int? companyId)
    {
        if (startDate.HasValue) query = query.Where(o => o.OrderDate >= startDate.Value);
        if (endDate.HasValue) query = query.Where(o => o.OrderDate <= endDate.Value);
        
        var currentCompanyId = _tenantService.GetCompanyId();
        if (currentCompanyId.HasValue) query = query.Where(o => o.CompanyId == currentCompanyId.Value);
        else if (companyId.HasValue) query = query.Where(o => o.CompanyId == companyId.Value);
        
        return query;
    }
    
    // Helper for Products filter
    private IQueryable<Product> FilterProducts(IQueryable<Product> query, int? companyId)
    {
        var currentCompanyId = _tenantService.GetCompanyId();
        if (currentCompanyId.HasValue) query = query.Where(p => p.CompanyId == currentCompanyId.Value);
        else if (companyId.HasValue) query = query.Where(p => p.CompanyId == companyId.Value);
        return query;
    }

    public async Task<DashboardKpiDto> GetDashboardKpiAsync(DateTime? startDate = null, DateTime? endDate = null, int? companyId = null)
    {
        return new DashboardKpiDto
        {
            Sales = await GetSalesKpiAsync(startDate, endDate, companyId),
            Orders = await GetOrdersKpiAsync(startDate, endDate, companyId),
            Customers = new CustomerKpiDto(), // Simplify for now or implement
            TopProducts = await GetTopProductsAsync(startDate, endDate, companyId),
            LowStockProducts = await GetLowStockProductsAsync(companyId),
            RevenueTrend = await GetRevenueTrendAsync(startDate, endDate, companyId),
            CustomerSegmentation = await GetCustomerSegmentsAsync(startDate, endDate, companyId),
            CategorySales = await GetCategorySalesAsync(startDate, endDate, companyId),
            GeographicDistribution = await GetGeographicDistributionAsync(startDate, endDate, companyId),
            AverageCartTrend = await GetAverageCartTrendAsync(startDate, endDate, companyId),
            OrderStatusDistribution = await GetOrderStatusDistributionAsync(startDate, endDate, companyId)
        };
    }

    public async Task<SalesKpiDto> GetSalesKpiAsync(DateTime? startDate, DateTime? endDate, int? companyId)
    {
        IQueryable<Order> query = _context.Orders.AsNoTracking();
        query = FilterOrders(query, startDate, endDate, companyId);
        
        var totalSales = await query.SumAsync(o => o.TotalAmount);
        var count = await query.CountAsync();
        
        // Simple logic for daily/weekly - simplified for this refactor to avoid complexity issues
        return new SalesKpiDto
        {
            DailySales = totalSales, // Placeholder logic
            AverageOrderValue = count > 0 ? totalSales / count : 0
        };
    }

    public async Task<OrderKpiDto> GetOrdersKpiAsync(DateTime? startDate, DateTime? endDate, int? companyId)
    {
         IQueryable<Order> query = _context.Orders.AsNoTracking();
         query = FilterOrders(query, startDate, endDate, companyId);
         
         var totalOrders = await query.CountAsync();
         // Count by status
         var pending = await query.CountAsync(o => o.Status == OrderStatus.Pending);
         var shipped = await query.CountAsync(o => o.Status == OrderStatus.Shipped);
         var delivered = await query.CountAsync(o => o.Status == OrderStatus.Delivered);
         var cancelled = await query.CountAsync(o => o.Status == OrderStatus.Cancelled);
         
         return new OrderKpiDto
         {
             TotalOrders = totalOrders,
             PendingCount = pending,
             ShippedCount = shipped,
             DeliveredCount = delivered,
             ReturnedCount = 0, // Not tracked
             CancelledCount = cancelled,
             PendingPercent = totalOrders > 0 ? (decimal)pending / totalOrders * 100 : 0,
             ShippedPercent = totalOrders > 0 ? (decimal)shipped / totalOrders * 100 : 0,
             DeliveredPercent = totalOrders > 0 ? (decimal)delivered / totalOrders * 100 : 0,
             ReturnedPercent = 0,
             CancelledPercent = totalOrders > 0 ? (decimal)cancelled / totalOrders * 100 : 0
         };
    }

    public async Task<List<TopProductDto>> GetTopProductsAsync(DateTime? startDate, DateTime? endDate, int? companyId)
    {
        // Need OrderItems for this. Assuming Order has Items.
        IQueryable<Order> query = _context.Orders.AsNoTracking().Include(o => o.Items).ThenInclude(i => i.Product).ThenInclude(p => p.Images);
        query = FilterOrders(query, startDate, endDate, companyId);

        // Flatten items
        var items = query.SelectMany(o => o.Items);
        
        return await items
            .GroupBy(i => i.ProductId)
            .Select(g => new TopProductDto
            {
                ProductId = g.Key,
                ProductName = g.First().Product.Name,
                QuantitySold = g.Sum(i => i.Quantity),
                Revenue = g.Sum(i => i.UnitPrice * i.Quantity),
                ImageUrl = g.First().Product.Images.FirstOrDefault(img => img.IsPrimary) != null 
                    ? g.First().Product.Images.FirstOrDefault(img => img.IsPrimary).ImageUrl 
                    : null
            })
            .OrderByDescending(x => x.Revenue)
            .Take(5)
            .ToListAsync();
    }

    public async Task<List<LowStockProductDto>> GetLowStockProductsAsync(int? companyId)
    {
        IQueryable<Product> query = _context.Products.AsNoTracking().Include(p => p.Category).Include(p => p.Images);
        query = FilterProducts(query, companyId);
        
        return await query
            .Where(p => p.StockQuantity < 10) // Threshold 10
            .Select(p => new LowStockProductDto
            {
                ProductId = p.Id,
                ProductName = p.Name,
                CurrentStock = p.StockQuantity,
                CategoryName = p.Category.Name,
                ImageUrl = p.Images.FirstOrDefault(i => i.IsPrimary) != null 
                    ? p.Images.FirstOrDefault(i => i.IsPrimary).ImageUrl 
                    : null
            })
            .ToListAsync();
    }

    public async Task<List<RevenueTrendDto>> GetRevenueTrendAsync(DateTime? startDate, DateTime? endDate, int? companyId)
    {
        IQueryable<Order> query = _context.Orders.AsNoTracking();
        query = FilterOrders(query, startDate, endDate, companyId);
        
        // Group by Date (only Date part)
        // Accessing Date property in EF might need .Date
        // SQLite supports it.
        
        return await query
            .GroupBy(o => o.OrderDate.Date)
            .Select(g => new RevenueTrendDto
            {
                Date = g.Key,
                Revenue = g.Sum(o => o.TotalAmount),
                OrderCount = g.Count()
            })
            .OrderBy(r => r.Date)
            .ToListAsync();
    }

    public async Task<CustomerSegmentationDto> GetCustomerSegmentsAsync(DateTime? startDate, DateTime? endDate, int? companyId)
    {
         // Placeholder
         return new CustomerSegmentationDto();
    }

    public async Task<List<CategorySalesDto>> GetCategorySalesAsync(DateTime? startDate, DateTime? endDate, int? companyId)
    {
        IQueryable<Order> query = _context.Orders.AsNoTracking().Include(o => o.Items).ThenInclude(i => i.Product).ThenInclude(p => p.Category);
        query = FilterOrders(query, startDate, endDate, companyId);
        
        var totalSales = await query.SumAsync(o => o.TotalAmount);
        if (totalSales == 0) totalSales = 1;

        var items = query.SelectMany(o => o.Items);
        
        return await items
            .GroupBy(i => i.Product.CategoryId)
            .Select(g => new CategorySalesDto
            {
                CategoryId = g.Key,
                CategoryName = g.First().Product.Category.Name,
                TotalSales = g.Sum(i => i.Quantity * i.UnitPrice),
                TotalQuantity = g.Sum(i => i.Quantity),
                Percentage = (g.Sum(i => i.Quantity * i.UnitPrice) / totalSales) * 100
            })
            .ToListAsync();
    }

    public async Task<List<GeographicDistributionDto>> GetGeographicDistributionAsync(DateTime? startDate, DateTime? endDate, int? companyId)
    {
         // Requires Address data in Order
         return new List<GeographicDistributionDto>();
    }

    public async Task<List<AverageCartTrendDto>> GetAverageCartTrendAsync(DateTime? startDate, DateTime? endDate, int? companyId)
    {
         IQueryable<Order> query = _context.Orders.AsNoTracking();
        query = FilterOrders(query, startDate, endDate, companyId);
        
        return await query
            .GroupBy(o => o.OrderDate.Date)
            .Select(g => new AverageCartTrendDto
            {
                Date = g.Key,
                AverageCartValue = g.Average(o => o.TotalAmount),
                OrderCount = g.Count()
            })
            .OrderBy(r => r.Date)
            .ToListAsync();
    }

    public async Task<List<OrderStatusDistributionDto>> GetOrderStatusDistributionAsync(DateTime? startDate, DateTime? endDate, int? companyId)
    {
        IQueryable<Order> query = _context.Orders.AsNoTracking();
        query = FilterOrders(query, startDate, endDate, companyId);

        return await query
            .GroupBy(o => o.OrderDate.Date)
            .Select(g => new OrderStatusDistributionDto
            {
                Date = g.Key,
                PendingCount = g.Count(o => o.Status == OrderStatus.Pending),
                ShippedCount = g.Count(o => o.Status == OrderStatus.Shipped),
                DeliveredCount = g.Count(o => o.Status == OrderStatus.Delivered),
                ReturnedCount = 0,
                CancelledCount = g.Count(o => o.Status == OrderStatus.Cancelled)
            })
            .OrderBy(r => r.Date)
            .ToListAsync();
    }
}
