namespace ECommerce.Application.DTOs;

/// <summary>
/// Günlük satış verisi
/// </summary>
public record DailySalesDto
{
    public DateTime Date { get; init; }
    public decimal TotalAmount { get; init; }
}

/// <summary>
/// Dashboard KPI ana DTO - tüm metrikleri içerir
/// </summary>
public record DashboardKpiDto
{
    public SalesKpiDto Sales { get; init; } = new();
    public OrderKpiDto Orders { get; init; } = new();
    public CustomerKpiDto Customers { get; init; } = new();
    public List<TopProductDto> TopProducts { get; init; } = new();
    public List<LowStockProductDto> LowStockProducts { get; init; } = new();
    public List<RevenueTrendDto> RevenueTrend { get; init; } = new();
    public CustomerSegmentationDto CustomerSegmentation { get; init; } = new();
    public List<CategorySalesDto> CategorySales { get; init; } = new();
    public List<CategoryStockDto> CategoryStock { get; init; } = new();
    public List<GeographicDistributionDto> GeographicDistribution { get; init; } = new();
    public List<AverageCartTrendDto> AverageCartTrend { get; init; } = new();
    public List<OrderStatusDistributionDto> OrderStatusDistribution { get; init; } = new();
}

/// <summary>
/// Satış KPI'ları
/// </summary>
public record SalesKpiDto
{
    public decimal DailySales { get; init; }
    public decimal DailySalesChange { get; init; }
    public decimal YesterdaySales { get; init; }
    public decimal WeeklySales { get; init; }
    public decimal WeeklySalesChange { get; init; }
    public decimal LastWeekSales { get; init; }
    public decimal MonthlySales { get; init; }
    public decimal MonthlySalesChange { get; init; }
    public decimal MonthlyTarget { get; init; }
    public decimal AverageOrderValue { get; init; }
    public decimal AverageOrderValueChange { get; init; }
}

/// <summary>
/// Sipariş KPI'ları
/// </summary>
public record OrderKpiDto
{
    public int TotalOrders { get; init; }
    public int DailyOrders { get; init; }
    public int PendingCount { get; init; }
    public int ShippedCount { get; init; }
    public int DeliveredCount { get; init; }
    public int ReturnedCount { get; init; }
    public int CancelledCount { get; init; }
    public decimal PendingPercent { get; init; }
    public decimal ShippedPercent { get; init; }
    public decimal DeliveredPercent { get; init; }
    public decimal ReturnedPercent { get; init; }
    public decimal CancelledPercent { get; init; }
}

/// <summary>
/// Müşteri KPI'ları
/// </summary>
public record CustomerKpiDto
{
    public int TotalCustomers { get; init; }
    public int DailyNewCustomers { get; init; }
    public int MonthlyNewCustomers { get; init; }
    public decimal CustomerGrowthRate { get; init; }
}

/// <summary>
/// En Çok Satan Ürün
/// </summary>
public record TopProductDto
{
    public int ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public string? ImageUrl { get; init; }
    public int QuantitySold { get; init; }
    public decimal Revenue { get; init; }
    public string CategoryName { get; init; } = string.Empty;
}

/// <summary>
/// Kritik Stok Ürünü
/// </summary>
public record LowStockProductDto
{
    public int ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public string? ImageUrl { get; init; }
    public int CurrentStock { get; init; }
    public int Threshold { get; init; }
    public decimal DailyAverageSales { get; init; }
    public int DaysUntilOutOfStock { get; init; }
    public string CategoryName { get; init; } = string.Empty;
    public string BrandName { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public bool IsCritical => CurrentStock < 5;
}

/// <summary>
/// Gelir Trend Verisi
/// </summary>
public record RevenueTrendDto
{
    public DateTime Date { get; init; }
    public decimal Revenue { get; init; }
    public int OrderCount { get; init; }
}

/// <summary>
/// Müşteri Segmentasyonu
/// </summary>
public record CustomerSegmentationDto
{
    public int NewCustomers { get; init; }
    public int ReturningCustomers { get; init; }
    public decimal NewCustomersRevenue { get; init; }
    public decimal ReturningCustomersRevenue { get; init; }
    private int TotalCustomers => NewCustomers + ReturningCustomers;
    public decimal NewCustomerPercent => TotalCustomers > 0 ? (decimal)NewCustomers / TotalCustomers * 100 : 0;
    public decimal ReturningCustomerPercent => TotalCustomers > 0 ? (decimal)ReturningCustomers / TotalCustomers * 100 : 0;
}

/// <summary>
/// Kategori bazlı satış dağılımı (Pie Chart için)
/// </summary>
public record CategorySalesDto
{
    public int CategoryId { get; init; }
    public string CategoryName { get; init; } = string.Empty;
    public decimal TotalSales { get; init; }
    public int TotalQuantity { get; init; }
    public decimal Percentage { get; init; }
    public string Color { get; init; } = string.Empty;
}

/// <summary>
/// Kategori bazlı stok dağılımı (Pie Chart için)
/// </summary>
public record CategoryStockDto
{
    public int CategoryId { get; init; }
    public string CategoryName { get; init; } = string.Empty;
    public int StockQuantity { get; init; }
    public decimal Percentage { get; init; }
    public string Color { get; init; } = string.Empty;
}

/// <summary>
/// Coğrafi dağılım (Heatmap için)
/// </summary>
public record GeographicDistributionDto
{
    public string City { get; init; } = string.Empty;
    public string Country { get; init; } = string.Empty;
    public string State { get; init; } = string.Empty;
    public int OrderCount { get; init; }
    public decimal TotalRevenue { get; init; }
    public decimal Percentage { get; init; }
    public string Intensity { get; init; } = "low";
    public double Latitude { get; init; }
    public double Longitude { get; init; }
}

/// <summary>
/// Ortalama sepet tutarı trendi (Line Chart için)
/// </summary>
public record AverageCartTrendDto
{
    public DateTime Date { get; init; }
    public decimal AverageCartValue { get; init; }
    public int OrderCount { get; init; }
}

/// <summary>
/// Sipariş durumu dağılımı (Stacked Bar Chart için)
/// </summary>
public record OrderStatusDistributionDto
{
    public DateTime Date { get; init; }
    public int PendingCount { get; init; }
    public int ShippedCount { get; init; }
    public int DeliveredCount { get; init; }
    public int ReturnedCount { get; init; }
    public int CancelledCount { get; init; }
}
