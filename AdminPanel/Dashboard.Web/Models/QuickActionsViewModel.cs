namespace Dashboard.Web.Models;

/// <summary>
/// Hızlı Ürün Ekleme için ViewModel
/// </summary>
public class QuickProductAddVm
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public int CategoryId { get; set; }
    public int BrandId { get; set; }
    public string? ImageUrl { get; set; }
}

/// <summary>
/// Hızlı Sipariş Güncelleme için ViewModel
/// </summary>
public class QuickOrderUpdateVm
{
    public int OrderId { get; set; }
    public string Status { get; set; } = string.Empty;
}

/// <summary>
/// Hızlı Mesaj Yanıtlama için ViewModel
/// </summary>
public class QuickMessageReplyVm
{
    public int MessageId { get; set; }
    public string Reply { get; set; } = string.Empty;
}

/// <summary>
/// Hızlı Aksiyon Dashboard özeti için ViewModel
/// </summary>
public class QuickActionsSummaryVm
{
    public int LowStockCount { get; set; }
    public int PendingOrdersCount { get; set; }
    public int ActiveCampaignsCount { get; set; }
    public int UnreadMessagesCount { get; set; }
}
