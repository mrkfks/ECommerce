namespace ECommerce.Application.DTOs.Dashboard;

public record DailySalesDto(
    DateTime Date,
    decimal TotalAmount
);

/// <summary>
/// Dashboard KPI ana DTO - tüm metrikleri içerir
/// </summary>
public record DashboardKpiDto(
    SalesKpiDto Sales,
    OrderKpiDto Orders,
    CustomerKpiDto Customers,
    List<TopProductDto> TopProducts,
    List<LowStockProductDto> LowStockProducts,
    List<RevenueTrendDto> RevenueTrend,
    CustomerSegmentationDto CustomerSegmentation,
    List<CategorySalesDto> CategorySales,
    List<CategoryStockDto> CategoryStock,
    List<GeographicDistributionDto> GeographicDistribution,
    List<AverageCartTrendDto> AverageCartTrend,
    List<OrderStatusDistributionDto> OrderStatusDistribution
);

/// <summary>
/// Satış KPI'ları
/// </summary>
public record SalesKpiDto(
    decimal DailySales = 0,
    decimal DailySalesChange = 0,
    decimal YesterdaySales = 0,
    decimal WeeklySales = 0,
    decimal WeeklySalesChange = 0,
    decimal LastWeekSales = 0,
    decimal MonthlySales = 0,
    decimal MonthlySalesChange = 0,
    decimal MonthlyTarget = 0,
    decimal AverageOrderValue = 0,
    decimal AverageOrderValueChange = 0
);

/// <summary>
/// Sipariş KPI'ları
/// </summary>
public record OrderKpiDto(
    int TotalOrders = 0,
    int DailyOrders = 0,
    int PendingCount = 0,
    int ShippedCount = 0,
    int DeliveredCount = 0,
    int ReturnedCount = 0,
    int CancelledCount = 0,
    decimal PendingPercent = 0,
    decimal ShippedPercent = 0,
    decimal DeliveredPercent = 0,
    decimal ReturnedPercent = 0,
    decimal CancelledPercent = 0
);

/// <summary>
/// Müşteri KPI'ları
/// </summary>
public record CustomerKpiDto(
    int TotalCustomers = 0,
    int DailyNewCustomers = 0,
    int MonthlyNewCustomers = 0,
    decimal CustomerGrowthRate = 0
);

/// <summary>
/// En Çok Satan Ürün
/// </summary>
public record TopProductDto(
    int ProductId,
    string ProductName,
    string? ImageUrl,
    int QuantitySold,
    decimal Revenue,
    string CategoryName
);

/// <summary>
/// Kritik Stok Ürünü
/// </summary>
public record LowStockProductDto(
    int ProductId,
    string ProductName,
    string? ImageUrl,
    int CurrentStock,
    decimal DailyAverageSales,
    int DaysUntilOutOfStock,
    string CategoryName
)
{
    public bool IsCritical => CurrentStock < 5;
}

/// <summary>
/// Gelir Trend Verisi
/// </summary>
public record RevenueTrendDto(
    DateTime Date,
    decimal Revenue,
    int OrderCount
);

/// <summary>
/// Müşteri Segmentasyonu
/// </summary>
public record CustomerSegmentationDto(
    int NewCustomers = 0,
    int ReturningCustomers = 0,
    decimal NewCustomersRevenue = 0,
    decimal ReturningCustomersRevenue = 0
)
{
    private int TotalCustomers => NewCustomers + ReturningCustomers;
    public decimal NewCustomerPercent => TotalCustomers > 0 ? (decimal)NewCustomers / TotalCustomers * 100 : 0;
    public decimal ReturningCustomerPercent => TotalCustomers > 0 ? (decimal)ReturningCustomers / TotalCustomers * 100 : 0;
}

/// <summary>
/// Kategori bazlı satış dağılımı (Pie Chart için)
/// </summary>
public record CategorySalesDto(
    int CategoryId,
    string CategoryName,
    decimal TotalSales,
    int TotalQuantity,
    decimal Percentage,
    string Color
);

/// <summary>
/// Kategori bazlı stok dağılımı (Pie Chart için)
/// </summary>
public record CategoryStockDto(
    int CategoryId,
    string CategoryName,
    int StockQuantity,
    decimal Percentage,
    string Color
);

/// <summary>
/// Coğrafi dağılım (Heatmap için)
/// </summary>
public record GeographicDistributionDto(
    string City,
    string State,
    int OrderCount,
    decimal TotalRevenue,
    decimal Percentage,
    string Intensity = "low"
);

/// <summary>
/// Ortalama sepet tutarı trendi (Line Chart için)
/// </summary>
public record AverageCartTrendDto(
    DateTime Date,
    decimal AverageCartValue,
    int OrderCount
);

/// <summary>
/// Sipariş durumu dağılımı (Stacked Bar Chart için)
/// </summary>
public record OrderStatusDistributionDto(
    DateTime Date,
    int PendingCount,
    int ShippedCount,
    int DeliveredCount,
    int ReturnedCount,
    int CancelledCount
);
