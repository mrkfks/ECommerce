using MediatR;
using Microsoft.EntityFrameworkCore;
using ECommerce.Application.DTOs.Dashboard;
using ECommerce.Application.Interfaces;
using ECommerce.Domain.Enums;

namespace ECommerce.Application.Features.Dashboard.Queries;

/// <summary>
/// Dashboard KPI verilerini hesaplayan handler
/// </summary>
public class GetDashboardKpiQueryHandler : IRequestHandler<GetDashboardKpiQuery, DashboardKpiDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantService _tenantService;

    public GetDashboardKpiQueryHandler(IApplicationDbContext context, ITenantService tenantService)
    {
        _context = context;
        _tenantService = tenantService;
    }

    public async Task<DashboardKpiDto> Handle(GetDashboardKpiQuery request, CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow.Date;
        var yesterday = today.AddDays(-1);
        var weekStart = today.AddDays(-7);
        var lastWeekStart = today.AddDays(-14);
        var monthStart = today.AddDays(-30);
        var lastMonthStart = today.AddDays(-60);

        var companyId = request.CompanyId ?? _tenantService.GetCompanyId();

        // Temel sorgular - Include ile ilişkili verileri de yükle
        var ordersQuery = _context.Orders
            .AsNoTracking()
            .Include(o => o.Address)
            .Include(o => o.Items)
                .ThenInclude(i => i.Product)
                    .ThenInclude(p => p!.Category)
            .AsQueryable();

        var productsQuery = _context.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .AsQueryable();

        var customersQuery = _context.Customers.AsNoTracking();

        // Company filtresi
        if (companyId.HasValue)
        {
            ordersQuery = ordersQuery.Where(o => o.CompanyId == companyId.Value);
            productsQuery = productsQuery.Where(p => p.CompanyId == companyId.Value);
            customersQuery = customersQuery.Where(c => c.CompanyId == companyId.Value);
        }

        var result = new DashboardKpiDto
        {
            Sales = await CalculateSalesKpi(ordersQuery, today, yesterday, weekStart, lastWeekStart, monthStart, cancellationToken),
            Orders = await CalculateOrderKpi(ordersQuery, today, cancellationToken),
            Customers = await CalculateCustomerKpi(customersQuery, today, monthStart, cancellationToken),
            TopProducts = await GetTopProducts(ordersQuery, monthStart, cancellationToken),
            LowStockProducts = await GetLowStockProducts(productsQuery, ordersQuery, cancellationToken),
            RevenueTrend = await GetRevenueTrend(ordersQuery, today, cancellationToken),
            CustomerSegmentation = await GetCustomerSegmentation(ordersQuery, customersQuery, monthStart, cancellationToken),
            CategorySales = await GetCategorySales(ordersQuery, monthStart, cancellationToken),
            GeographicDistribution = await GetGeographicDistribution(ordersQuery, monthStart, cancellationToken),
            AverageCartTrend = await GetAverageCartTrend(ordersQuery, today, cancellationToken),
            OrderStatusDistribution = await GetOrderStatusDistribution(ordersQuery, today, cancellationToken)
        };

        return result;
    }

    private async Task<SalesKpiDto> CalculateSalesKpi(
        IQueryable<Domain.Entities.Order> orders,
        DateTime today, DateTime yesterday, DateTime weekStart, DateTime lastWeekStart, DateTime monthStart,
        CancellationToken ct)
    {
        // Günlük satışlar
        var dailySales = await orders
            .Where(o => o.OrderDate.Date == today && o.Status != OrderStatus.Cancelled)
            .SumAsync(o => o.TotalAmount, ct);

        var yesterdaySales = await orders
            .Where(o => o.OrderDate.Date == yesterday && o.Status != OrderStatus.Cancelled)
            .SumAsync(o => o.TotalAmount, ct);

        // Haftalık satışlar
        var weeklySales = await orders
            .Where(o => o.OrderDate.Date >= weekStart && o.Status != OrderStatus.Cancelled)
            .SumAsync(o => o.TotalAmount, ct);

        var lastWeekSales = await orders
            .Where(o => o.OrderDate.Date >= lastWeekStart && o.OrderDate.Date < weekStart && o.Status != OrderStatus.Cancelled)
            .SumAsync(o => o.TotalAmount, ct);

        // Aylık satışlar
        var monthlySales = await orders
            .Where(o => o.OrderDate.Date >= monthStart && o.Status != OrderStatus.Cancelled)
            .SumAsync(o => o.TotalAmount, ct);

        var lastMonthSales = await orders
            .Where(o => o.OrderDate.Date >= monthStart.AddDays(-30) && o.OrderDate.Date < monthStart && o.Status != OrderStatus.Cancelled)
            .SumAsync(o => o.TotalAmount, ct);

        // Ortalama sipariş değeri
        var avgOrderValue = await orders
            .Where(o => o.OrderDate.Date >= monthStart && o.Status != OrderStatus.Cancelled)
            .AverageAsync(o => (decimal?)o.TotalAmount, ct) ?? 0;

        var lastMonthAvg = await orders
            .Where(o => o.OrderDate.Date >= monthStart.AddDays(-30) && o.OrderDate.Date < monthStart && o.Status != OrderStatus.Cancelled)
            .AverageAsync(o => (decimal?)o.TotalAmount, ct) ?? 0;

        return new SalesKpiDto
        {
            DailySales = dailySales,
            YesterdaySales = yesterdaySales,
            DailySalesChange = yesterdaySales > 0 ? ((dailySales - yesterdaySales) / yesterdaySales) * 100 : 0,

            WeeklySales = weeklySales,
            LastWeekSales = lastWeekSales,
            WeeklySalesChange = lastWeekSales > 0 ? ((weeklySales - lastWeekSales) / lastWeekSales) * 100 : 0,

            MonthlySales = monthlySales,
            MonthlySalesChange = lastMonthSales > 0 ? ((monthlySales - lastMonthSales) / lastMonthSales) * 100 : 0,
            MonthlyTarget = monthlySales * 1.1m, // %10 üstü hedef

            AverageOrderValue = avgOrderValue,
            AverageOrderValueChange = lastMonthAvg > 0 ? ((avgOrderValue - lastMonthAvg) / lastMonthAvg) * 100 : 0
        };
    }

    private async Task<OrderKpiDto> CalculateOrderKpi(
        IQueryable<Domain.Entities.Order> orders,
        DateTime today,
        CancellationToken ct)
    {
        var totalOrders = await orders.CountAsync(ct);
        var dailyOrders = await orders.Where(o => o.OrderDate.Date == today).CountAsync(ct);

        var statusCounts = await orders
            .GroupBy(o => o.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        var pending = statusCounts.FirstOrDefault(s => s.Status == OrderStatus.Pending)?.Count ?? 0;
        var processing = statusCounts.FirstOrDefault(s => s.Status == OrderStatus.Processing)?.Count ?? 0;
        var shipped = statusCounts.FirstOrDefault(s => s.Status == OrderStatus.Shipped)?.Count ?? 0;
        var delivered = statusCounts.FirstOrDefault(s => s.Status == OrderStatus.Delivered)?.Count ?? 0;
        var cancelled = statusCounts.FirstOrDefault(s => s.Status == OrderStatus.Cancelled)?.Count ?? 0;

        var total = (decimal)(pending + processing + shipped + delivered + cancelled);

        return new OrderKpiDto
        {
            TotalOrders = totalOrders,
            DailyOrders = dailyOrders,
            PendingCount = pending + processing,
            ShippedCount = shipped,
            DeliveredCount = delivered,
            CancelledCount = cancelled,
            ReturnedCount = 0, // Returned status yok, 0 olarak bırak
            PendingPercent = total > 0 ? (pending + processing) / total * 100 : 0,
            ShippedPercent = total > 0 ? shipped / total * 100 : 0,
            DeliveredPercent = total > 0 ? delivered / total * 100 : 0,
            CancelledPercent = total > 0 ? cancelled / total * 100 : 0,
            ReturnedPercent = 0
        };
    }

    private async Task<CustomerKpiDto> CalculateCustomerKpi(
        IQueryable<Domain.Entities.Customer> customers,
        DateTime today, DateTime monthStart,
        CancellationToken ct)
    {
        var totalCustomers = await customers.CountAsync(ct);
        var dailyNew = await customers.Where(c => c.CreatedAt.Date == today).CountAsync(ct);
        var monthlyNew = await customers.Where(c => c.CreatedAt.Date >= monthStart).CountAsync(ct);

        var lastMonthNew = await customers
            .Where(c => c.CreatedAt.Date >= monthStart.AddDays(-30) && c.CreatedAt.Date < monthStart)
            .CountAsync(ct);

        var growthRate = lastMonthNew > 0 ? ((decimal)(monthlyNew - lastMonthNew) / lastMonthNew) * 100 : 0;

        return new CustomerKpiDto
        {
            TotalCustomers = totalCustomers,
            DailyNewCustomers = dailyNew,
            MonthlyNewCustomers = monthlyNew,
            CustomerGrowthRate = growthRate
        };
    }

    private async Task<List<TopProductDto>> GetTopProducts(
        IQueryable<Domain.Entities.Order> orders,
        DateTime monthStart,
        CancellationToken ct)
    {
        var topProducts = await orders
            .Where(o => o.OrderDate.Date >= monthStart && o.Status != OrderStatus.Cancelled)
            .SelectMany(o => o.Items)
            .Where(i => i.Product != null && i.Product.Category != null)
            .GroupBy(i => new
            {
                i.ProductId,
                Name = i.Product != null ? i.Product.Name : string.Empty,
                ImageUrl = i.Product != null ? i.Product.ImageUrl : string.Empty,
                CategoryName = i.Product != null && i.Product.Category != null ? i.Product.Category.Name : string.Empty
            })
            .Select(g => new TopProductDto
            {
                ProductId = g.Key.ProductId,
                ProductName = g.Key.Name,
                ImageUrl = g.Key.ImageUrl,
                CategoryName = g.Key.CategoryName,
                QuantitySold = g.Sum(i => i.Quantity),
                Revenue = g.Sum(i => i.Quantity * i.UnitPrice)
            })
            .OrderByDescending(p => p.QuantitySold)
            .Take(5)
            .ToListAsync(ct);

        return topProducts;
    }

    private async Task<List<LowStockProductDto>> GetLowStockProducts(
        IQueryable<Domain.Entities.Product> products,
        IQueryable<Domain.Entities.Order> orders,
        CancellationToken ct)
    {
        var thirtyDaysAgo = DateTime.UtcNow.Date.AddDays(-30);

        // Düşük stoklu ürünler
        var lowStockProducts = await products
            .Where(p => p.StockQuantity < 10 && p.IsActive)
            .Select(p => new
            {
                p.Id,
                p.Name,
                p.ImageUrl,
                p.StockQuantity,
                CategoryName = p.Category != null ? p.Category.Name : string.Empty
            })
            .ToListAsync(ct);

        // Son 30 günlük satış ortalamaları
        var salesData = await orders
            .Where(o => o.OrderDate.Date >= thirtyDaysAgo && o.Status != OrderStatus.Cancelled)
            .SelectMany(o => o.Items)
            .Where(i => lowStockProducts.Select(p => p.Id).Contains(i.ProductId))
            .GroupBy(i => i.ProductId)
            .Select(g => new { ProductId = g.Key, TotalSold = g.Sum(i => i.Quantity) })
            .ToListAsync(ct);

        return lowStockProducts.Select(p =>
        {
            var dailyAvg = salesData.FirstOrDefault(s => s.ProductId == p.Id)?.TotalSold / 30m ?? 0;
            return new LowStockProductDto
            {
                ProductId = p.Id,
                ProductName = p.Name,
                ImageUrl = p.ImageUrl,
                CurrentStock = p.StockQuantity,
                CategoryName = p.CategoryName ?? string.Empty,
                DailyAverageSales = dailyAvg,
                DaysUntilOutOfStock = dailyAvg > 0 ? (int)(p.StockQuantity / dailyAvg) : 999
            };
        })
        .OrderBy(p => p.CurrentStock)
        .Take(10)
        .ToList();
    }

    private async Task<List<RevenueTrendDto>> GetRevenueTrend(
        IQueryable<Domain.Entities.Order> orders,
        DateTime today,
        CancellationToken ct)
    {
        var thirtyDaysAgo = today.AddDays(-30);

        var trend = await orders
            .Where(o => o.OrderDate.Date >= thirtyDaysAgo && o.Status != OrderStatus.Cancelled)
            .GroupBy(o => o.OrderDate.Date)
            .Select(g => new RevenueTrendDto
            {
                Date = g.Key,
                Revenue = g.Sum(o => o.TotalAmount),
                OrderCount = g.Count()
            })
            .OrderBy(t => t.Date)
            .ToListAsync(ct);

        // Eksik günleri 0 ile doldur
        var result = new List<RevenueTrendDto>();
        for (var date = thirtyDaysAgo; date <= today; date = date.AddDays(1))
        {
            var existing = trend.FirstOrDefault(t => t.Date.Date == date.Date);
            result.Add(existing ?? new RevenueTrendDto { Date = date, Revenue = 0, OrderCount = 0 });
        }

        return result;
    }

    private async Task<CustomerSegmentationDto> GetCustomerSegmentation(
        IQueryable<Domain.Entities.Order> orders,
        IQueryable<Domain.Entities.Customer> customers,
        DateTime monthStart,
        CancellationToken ct)
    {
        // Son 30 günde sipariş veren müşteriler
        var recentOrders = await orders
            .Where(o => o.OrderDate.Date >= monthStart && o.Status != OrderStatus.Cancelled)
            .Select(o => new { o.CustomerId, o.TotalAmount })
            .ToListAsync(ct);

        var customerOrderCounts = recentOrders
            .GroupBy(o => o.CustomerId)
            .Select(g => new { CustomerId = g.Key, OrderCount = g.Count(), TotalRevenue = g.Sum(o => o.TotalAmount) })
            .ToList();

        // Tüm zamanlar için sipariş sayıları
        var allTimeOrderCounts = await orders
            .Where(o => o.Status != OrderStatus.Cancelled)
            .GroupBy(o => o.CustomerId)
            .Select(g => new { CustomerId = g.Key, TotalOrders = g.Count() })
            .ToListAsync(ct);

        var newCustomerIds = allTimeOrderCounts.Where(c => c.TotalOrders == 1).Select(c => c.CustomerId).ToHashSet();
        var returningCustomerIds = allTimeOrderCounts.Where(c => c.TotalOrders > 1).Select(c => c.CustomerId).ToHashSet();

        var newCustomersThisMonth = customerOrderCounts.Where(c => newCustomerIds.Contains(c.CustomerId)).ToList();
        var returningCustomersThisMonth = customerOrderCounts.Where(c => returningCustomerIds.Contains(c.CustomerId)).ToList();

        return new CustomerSegmentationDto
        {
            NewCustomers = newCustomersThisMonth.Count,
            ReturningCustomers = returningCustomersThisMonth.Count,
            NewCustomersRevenue = newCustomersThisMonth.Sum(c => c.TotalRevenue),
            ReturningCustomersRevenue = returningCustomersThisMonth.Sum(c => c.TotalRevenue)
        };
    }

    /// <summary>
    /// Kategori bazlı satış dağılımını hesaplar (Pie Chart için)
    /// </summary>
    private async Task<List<CategorySalesDto>> GetCategorySales(
        IQueryable<Domain.Entities.Order> orders,
        DateTime monthStart,
        CancellationToken ct)
    {
        var categorySales = await orders
            .Where(o => o.OrderDate.Date >= monthStart && o.Status != OrderStatus.Cancelled)
            .SelectMany(o => o.Items)
            .Where(i => i.Product != null && i.Product.Category != null)
            .GroupBy(i => new
            {
                CategoryId = i.Product != null ? i.Product.CategoryId : 0,
                CategoryName = i.Product != null && i.Product.Category != null ? i.Product.Category.Name : string.Empty
            })
            .Select(g => new
            {
                g.Key.CategoryId,
                g.Key.CategoryName,
                TotalSales = g.Sum(i => i.Quantity * i.UnitPrice),
                TotalQuantity = g.Sum(i => i.Quantity)
            })
                .ToListAsync(ct);

        categorySales = categorySales
            .OrderByDescending(c => c.TotalSales)
            .Take(10)
            .ToList();

        var grandTotal = categorySales.Sum(c => c.TotalSales);

        // Modern renk paleti
        var colors = new[] { "#3B82F6", "#10B981", "#F59E0B", "#EF4444", "#8B5CF6", "#EC4899", "#06B6D4", "#84CC16", "#F97316", "#6366F1" };

        return categorySales.Select((c, index) => new CategorySalesDto
        {
            CategoryId = c.CategoryId,
            CategoryName = c.CategoryName ?? string.Empty,
            TotalSales = c.TotalSales,
            TotalQuantity = c.TotalQuantity,
            Percentage = grandTotal > 0 ? (c.TotalSales / grandTotal) * 100 : 0,
            Color = colors[index % colors.Length]
        }).ToList();
    }

    /// <summary>
    /// Coğrafi dağılımı hesaplar (Heatmap için)
    /// </summary>
    private async Task<List<GeographicDistributionDto>> GetGeographicDistribution(
        IQueryable<Domain.Entities.Order> orders,
        DateTime monthStart,
        CancellationToken ct)
    {
        var distribution = await orders
            .Where(o => o.OrderDate.Date >= monthStart && o.Status != OrderStatus.Cancelled && o.Address != null)
            .GroupBy(o => new { o.Address!.City, o.Address.State })
            .Select(g => new
            {
                g.Key.City,
                g.Key.State,
                OrderCount = g.Count(),
                TotalRevenue = g.Sum(o => o.TotalAmount)
            })
            .OrderByDescending(d => d.OrderCount)
            .Take(20)
            .ToListAsync(ct);

        var maxOrders = distribution.Any() ? distribution.Max(d => d.OrderCount) : 0;
        var totalOrders = distribution.Sum(d => d.OrderCount);

        return distribution.Select(d =>
        {
            var ratio = maxOrders > 0 ? (decimal)d.OrderCount / maxOrders : 0;
            var intensity = ratio switch
            {
                >= 0.75m => "critical",
                >= 0.50m => "high",
                >= 0.25m => "medium",
                _ => "low"
            };

            return new GeographicDistributionDto
            {
                City = d.City,
                State = d.State,
                OrderCount = d.OrderCount,
                TotalRevenue = d.TotalRevenue,
                Percentage = totalOrders > 0 ? (decimal)d.OrderCount / totalOrders * 100 : 0,
                Intensity = intensity
            };
        }).ToList();
    }

    /// <summary>
    /// Ortalama sepet tutarı trendini hesaplar (Line Chart için)
    /// </summary>
    private async Task<List<AverageCartTrendDto>> GetAverageCartTrend(
        IQueryable<Domain.Entities.Order> orders,
        DateTime today,
        CancellationToken ct)
    {
        var thirtyDaysAgo = today.AddDays(-30);

        var trend = await orders
            .Where(o => o.OrderDate.Date >= thirtyDaysAgo && o.Status != OrderStatus.Cancelled)
            .GroupBy(o => o.OrderDate.Date)
            .Select(g => new
            {
                Date = g.Key,
                AverageCartValue = g.Average(o => o.TotalAmount),
                OrderCount = g.Count()
            })
            .OrderBy(t => t.Date)
            .ToListAsync(ct);

        // Eksik günleri önceki günün değeriyle doldur
        var result = new List<AverageCartTrendDto>();
        decimal lastValue = 0;

        for (var date = thirtyDaysAgo; date <= today; date = date.AddDays(1))
        {
            var existing = trend.FirstOrDefault(t => t.Date.Date == date.Date);
            if (existing != null)
            {
                lastValue = existing.AverageCartValue;
                result.Add(new AverageCartTrendDto
                {
                    Date = date,
                    AverageCartValue = existing.AverageCartValue,
                    OrderCount = existing.OrderCount
                });
            }
            else
            {
                result.Add(new AverageCartTrendDto
                {
                    Date = date,
                    AverageCartValue = lastValue,
                    OrderCount = 0
                });
            }
        }

        return result;
    }

    /// <summary>
    /// Sipariş durumu dağılımını zaman bazlı hesaplar (Stacked Bar Chart için)
    /// </summary>
    private async Task<List<OrderStatusDistributionDto>> GetOrderStatusDistribution(
        IQueryable<Domain.Entities.Order> orders,
        DateTime today,
        CancellationToken ct)
    {
        var thirtyDaysAgo = today.AddDays(-30);

        var distribution = await orders
            .Where(o => o.OrderDate.Date >= thirtyDaysAgo)
            .GroupBy(o => new { Date = o.OrderDate.Date, o.Status })
            .Select(g => new
            {
                g.Key.Date,
                g.Key.Status,
                Count = g.Count()
            })
            .ToListAsync(ct);

        var result = new List<OrderStatusDistributionDto>();

        for (var date = thirtyDaysAgo; date <= today; date = date.AddDays(1))
        {
            var dayData = distribution.Where(d => d.Date.Date == date.Date).ToList();

            result.Add(new OrderStatusDistributionDto
            {
                Date = date,
                PendingCount = dayData.Where(d => d.Status == OrderStatus.Pending || d.Status == OrderStatus.Processing).Sum(d => d.Count),
                ShippedCount = dayData.Where(d => d.Status == OrderStatus.Shipped).Sum(d => d.Count),
                DeliveredCount = dayData.Where(d => d.Status == OrderStatus.Delivered).Sum(d => d.Count),
                ReturnedCount = 0, // Returned status yok
                CancelledCount = dayData.Where(d => d.Status == OrderStatus.Cancelled).Sum(d => d.Count)
            });
        }

        return result;
    }
}
