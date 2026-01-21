using ECommerce.Application.DTOs.Dashboard;
using ECommerce.Application.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
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

    private readonly ICacheService _cacheService;

    public DashboardService(AppDbContext context, ITenantService tenantService, ILogger<DashboardService> logger, ICacheService cacheService)
    {
        _context = context;
        _tenantService = tenantService;
        _logger = logger;
        _cacheService = cacheService;
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
        var tenantId = companyId ?? _tenantService.GetCompanyId() ?? 0;
        var cacheKey = $"stats_{tenantId}";

        try
        {
            var cachedStats = await _cacheService.GetAsync<DashboardKpiDto>(cacheKey);
            if (cachedStats != null)
            {
                return cachedStats;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Dashboard cache read error");
        }

        var stats = new DashboardKpiDto
        {
            Sales = await GetSalesKpiAsync(startDate, endDate, companyId),
            Orders = await GetOrdersKpiAsync(startDate, endDate, companyId),
            Customers = new CustomerKpiDto(), // Placeholder
            TopProducts = await GetTopProductsAsync(startDate, endDate, companyId),
            LowStockProducts = await GetLowStockProductsAsync(companyId),
            RevenueTrend = await GetRevenueTrendAsync(startDate, endDate, companyId),
            CustomerSegmentation = await GetCustomerSegmentsAsync(startDate, endDate, companyId),
            CategorySales = await GetCategorySalesAsync(startDate, endDate, companyId),
            GeographicDistribution = await GetGeographicDistributionAsync(startDate, endDate, companyId),
            AverageCartTrend = await GetAverageCartTrendAsync(startDate, endDate, companyId),
            OrderStatusDistribution = await GetOrderStatusDistributionAsync(startDate, endDate, companyId)
        };

        try
        {
            await _cacheService.SetAsync(cacheKey, stats, TimeSpan.FromMinutes(15));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Dashboard cache write error");
        }

        return stats;
    }

    public async Task<SalesKpiDto> GetSalesKpiAsync(DateTime? startDate, DateTime? endDate, int? companyId)
    {
        return await GetCachedDataAsync($"SalesKpi_{companyId ?? 0}_{startDate}_{endDate}", async () => 
        {
            var today = DateTime.UtcNow.Date;
            var yesterday = today.AddDays(-1);

            IQueryable<Order> query = _context.Orders.AsNoTracking();
            query = FilterOrders(query, null, null, companyId);
            
            var todaySales = await query.Where(o => o.OrderDate >= today).SumAsync(o => o.TotalAmount);
            var yesterdaySales = await query.Where(o => o.OrderDate >= yesterday && o.OrderDate < today).SumAsync(o => o.TotalAmount);
            
            var totalSalesQuery = FilterOrders(_context.Orders.AsNoTracking(), startDate, endDate, companyId);
            var totalSales = await totalSalesQuery.SumAsync(o => o.TotalAmount);
            var count = await totalSalesQuery.CountAsync();
            
            decimal change = 0;
            if (yesterdaySales > 0)
                change = ((todaySales - yesterdaySales) / yesterdaySales) * 100;
            else if (todaySales > 0)
                change = 100;

            return new SalesKpiDto
            {
                DailySales = todaySales,
                YesterdaySales = yesterdaySales,
                DailySalesChange = Math.Round(change, 2),
                AverageOrderValue = count > 0 ? totalSales / count : 0,
                MonthlySales = totalSales // Contextual to full range passed or total
            };
        }, companyId);
    }

    public async Task<OrderKpiDto> GetOrdersKpiAsync(DateTime? startDate, DateTime? endDate, int? companyId)
    {
        return await GetCachedDataAsync($"OrdersKpi_{companyId ?? 0}_{startDate}_{endDate}", async () =>
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
        }, companyId);
    }

    public async Task<List<TopProductDto>> GetTopProductsAsync(DateTime? startDate, DateTime? endDate, int? companyId)
    {
        return await GetCachedDataAsync($"TopProducts_{companyId ?? 0}_{startDate}_{endDate}", async () =>
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
        }, companyId);
    }

    public async Task<List<LowStockProductDto>> GetLowStockProductsAsync(int? companyId)
    {
        return await GetCachedDataAsync($"LowStock_{companyId ?? 0}", async () => 
        {
            IQueryable<Product> query = _context.Products.AsNoTracking().Include(p => p.Category).Include(p => p.Images);
            query = FilterProducts(query, companyId);
            
            return await query
                .Where(p => p.StockQuantity < 5) // Threshold 5 as requested
                .Select(p => new LowStockProductDto
                {
                    ProductId = p.Id,
                    ProductName = p.Name,
                    CurrentStock = p.StockQuantity,
                    CategoryName = p.Category.Name,
                    ImageUrl = p.Images.FirstOrDefault(i => i.IsPrimary) != null 
                        ? p.Images.FirstOrDefault(i => i.IsPrimary).ImageUrl 
                        : null,
                    DaysUntilOutOfStock = 0 // Complex calc omitted for brevity
                })
                .ToListAsync();
        }, companyId);
    }

    public async Task<List<RevenueTrendDto>> GetRevenueTrendAsync(DateTime? startDate, DateTime? endDate, int? companyId)
    {
        return await GetCachedDataAsync($"RevenueTrend_{companyId ?? 0}_{startDate}_{endDate}", async () =>
        {
            var end = endDate ?? DateTime.UtcNow.Date;
            var start = startDate ?? end.AddDays(-29); // Last 30 days including today

            IQueryable<Order> query = _context.Orders.AsNoTracking();
            query = FilterOrders(query, start, end, companyId);
            
            var dailyData = await query
                .GroupBy(o => o.OrderDate.Date)
                .Select(g => new 
                {
                    Date = g.Key,
                    Revenue = g.Sum(o => o.TotalAmount),
                    Count = g.Count()
                })
                .ToListAsync();

            var result = new List<RevenueTrendDto>();
            for (var date = start; date <= end; date = date.AddDays(1))
            {
                var dayData = dailyData.FirstOrDefault(d => d.Date == date);
                result.Add(new RevenueTrendDto
                {
                    Date = date,
                    Revenue = dayData?.Revenue ?? 0,
                    OrderCount = dayData?.Count ?? 0
                });
            }
            
            return result;
        }, companyId);
    }

    public async Task<CustomerSegmentationDto> GetCustomerSegmentsAsync(DateTime? startDate, DateTime? endDate, int? companyId)
    {
         // Placeholder
         return await Task.FromResult(new CustomerSegmentationDto());
    }

    public async Task<List<CategorySalesDto>> GetCategorySalesAsync(DateTime? startDate, DateTime? endDate, int? companyId)
    {
        return await GetCachedDataAsync($"CategorySales_{companyId ?? 0}_{startDate}_{endDate}", async () =>
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
        }, companyId);
    }

    public async Task<List<GeographicDistributionDto>> GetGeographicDistributionAsync(DateTime? startDate, DateTime? endDate, int? companyId)
    {
         // Requires Address data in Order
         return new List<GeographicDistributionDto>();
    }

    public async Task<List<AverageCartTrendDto>> GetAverageCartTrendAsync(DateTime? startDate, DateTime? endDate, int? companyId)
    {
        return await GetCachedDataAsync($"AverageCartTrend_{companyId ?? 0}_{startDate}_{endDate}", async () =>
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
        }, companyId);
    }

    public async Task<List<OrderStatusDistributionDto>> GetOrderStatusDistributionAsync(DateTime? startDate, DateTime? endDate, int? companyId)
    {
        return await GetCachedDataAsync($"OrderStatusDistribution_{companyId ?? 0}_{startDate}_{endDate}", async () =>
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
        }, companyId);
    }
    private async Task<T> GetCachedDataAsync<T>(string key, Func<Task<T>> factory, int? companyId)
    {
         var tenantId = companyId ?? _tenantService.GetCompanyId() ?? 0;
         var cacheKey = $"dashboard_stats_{tenantId}_{key}";
         
         try 
         {
             var cachedData = await _cacheService.GetAsync<T>(cacheKey);
             if (cachedData != null)
             {
                 return cachedData;
             }
         }
         catch (Exception ex)
         {
             _logger.LogError(ex, "Cache read error for key {Key}", cacheKey);
         }
         
         var data = await factory();
         
         try
         {
             await _cacheService.SetAsync(cacheKey, data, TimeSpan.FromMinutes(15));
         }
         catch (Exception ex)
         {
             _logger.LogError(ex, "Cache write error for key {Key}", cacheKey);
         }
         
         return data;
    }
}
