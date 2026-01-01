namespace ECommerce.Application.DTOs.Dashboard;

/// <summary>
/// Dashboard KPI ana DTO - tüm metrikleri içerir
/// </summary>
public class DashboardKpiDto
{
    // Satış KPI'ları
    public SalesKpiDto Sales { get; set; } = new();

    // Sipariş Metrikleri
    public OrderKpiDto Orders { get; set; } = new();

    // Müşteri Metrikleri
    public CustomerKpiDto Customers { get; set; } = new();

    // En Çok Satan Ürünler
    public List<TopProductDto> TopProducts { get; set; } = new();

    // Kritik Stok Ürünleri
    public List<LowStockProductDto> LowStockProducts { get; set; } = new();

    // Gelir Trendi (Son 30 Gün)
    public List<RevenueTrendDto> RevenueTrend { get; set; } = new();

    // Müşteri Segmentasyonu
    public CustomerSegmentationDto CustomerSegmentation { get; set; } = new();

    // Kategori bazlı satış dağılımı
    public List<CategorySalesDto> CategorySales { get; set; } = new();

    // Coğrafi dağılım
    public List<GeographicDistributionDto> GeographicDistribution { get; set; } = new();

    // Ortalama sepet tutarı trendi
    public List<AverageCartTrendDto> AverageCartTrend { get; set; } = new();

    // Sipariş durumu zaman bazlı dağılımı
    public List<OrderStatusDistributionDto> OrderStatusDistribution { get; set; } = new();
}

/// <summary>
/// Satış KPI'ları
/// </summary>
public class SalesKpiDto
{
    public decimal DailySales { get; set; }
    public decimal DailySalesChange { get; set; } // Yüzde değişim
    public decimal YesterdaySales { get; set; }

    public decimal WeeklySales { get; set; }
    public decimal WeeklySalesChange { get; set; }
    public decimal LastWeekSales { get; set; }

    public decimal MonthlySales { get; set; }
    public decimal MonthlySalesChange { get; set; }
    public decimal MonthlyTarget { get; set; }

    public decimal AverageOrderValue { get; set; }
    public decimal AverageOrderValueChange { get; set; }
}

/// <summary>
/// Sipariş KPI'ları
/// </summary>
public class OrderKpiDto
{
    public int TotalOrders { get; set; }
    public int DailyOrders { get; set; }

    // Durum Dağılımı
    public int PendingCount { get; set; }
    public int ShippedCount { get; set; }
    public int DeliveredCount { get; set; }
    public int ReturnedCount { get; set; }
    public int CancelledCount { get; set; }

    // Yüzde Dağılımı
    public decimal PendingPercent { get; set; }
    public decimal ShippedPercent { get; set; }
    public decimal DeliveredPercent { get; set; }
    public decimal ReturnedPercent { get; set; }
    public decimal CancelledPercent { get; set; }
}

/// <summary>
/// Müşteri KPI'ları
/// </summary>
public class CustomerKpiDto
{
    public int TotalCustomers { get; set; }
    public int DailyNewCustomers { get; set; }
    public int MonthlyNewCustomers { get; set; }
    public decimal CustomerGrowthRate { get; set; } // Aylık büyüme oranı
}

/// <summary>
/// En Çok Satan Ürün
/// </summary>
public class TopProductDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public int QuantitySold { get; set; }
    public decimal Revenue { get; set; }
    public string CategoryName { get; set; } = string.Empty;
}

/// <summary>
/// Kritik Stok Ürünü
/// </summary>
public class LowStockProductDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public int CurrentStock { get; set; }
    public decimal DailyAverageSales { get; set; }
    public int DaysUntilOutOfStock { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public bool IsCritical => CurrentStock < 5;
}

/// <summary>
/// Gelir Trend Verisi
/// </summary>
public class RevenueTrendDto
{
    public DateTime Date { get; set; }
    public decimal Revenue { get; set; }
    public int OrderCount { get; set; }
}

/// <summary>
/// Müşteri Segmentasyonu
/// </summary>
public class CustomerSegmentationDto
{
    public int NewCustomers { get; set; } // İlk kez alışveriş yapanlar
    public int ReturningCustomers { get; set; } // Tekrar alışveriş yapanlar
    public decimal NewCustomersRevenue { get; set; }
    public decimal ReturningCustomersRevenue { get; set; }

    public decimal NewCustomerPercent => TotalCustomers > 0 ? (decimal)NewCustomers / TotalCustomers * 100 : 0;
    public decimal ReturningCustomerPercent => TotalCustomers > 0 ? (decimal)ReturningCustomers / TotalCustomers * 100 : 0;
    private int TotalCustomers => NewCustomers + ReturningCustomers;
}

/// <summary>
/// Kategori bazlı satış dağılımı (Pie Chart için)
/// </summary>
public class CategorySalesDto
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public decimal TotalSales { get; set; }
    public int TotalQuantity { get; set; }
    public decimal Percentage { get; set; }
    public string Color { get; set; } = string.Empty;
}

/// <summary>
/// Coğrafi dağılım (Heatmap için)
/// </summary>
public class GeographicDistributionDto
{
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public int OrderCount { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal Percentage { get; set; }
    public string Intensity { get; set; } = "low"; // low, medium, high, critical
}

/// <summary>
/// Ortalama sepet tutarı trendi (Line Chart için)
/// </summary>
public class AverageCartTrendDto
{
    public DateTime Date { get; set; }
    public decimal AverageCartValue { get; set; }
    public int OrderCount { get; set; }
}

/// <summary>
/// Sipariş durumu dağılımı (Stacked Bar Chart için)
/// </summary>
public class OrderStatusDistributionDto
{
    public DateTime Date { get; set; }
    public int PendingCount { get; set; }
    public int ShippedCount { get; set; }
    public int DeliveredCount { get; set; }
    public int ReturnedCount { get; set; }
    public int CancelledCount { get; set; }
}
