using Dashboard.Web.Services;

namespace Dashboard.Web.Models;

/// <summary>
/// Bildirimler sayfası ViewModel
/// </summary>
public class NotificationsViewModel
{
    /// <summary>
    /// Bildirim özeti
    /// </summary>
    public NotificationSummaryVm Summary { get; set; } = new();

    /// <summary>
    /// Tüm bildirimler
    /// </summary>
    public List<NotificationVm> AllNotifications { get; set; } = new();

    /// <summary>
    /// Düşük stoklu ürünler
    /// </summary>
    public List<LowStockItemVm> LowStockProducts { get; set; } = new();

    /// <summary>
    /// Son siparişler
    /// </summary>
    public List<RecentOrderVm> RecentOrders { get; set; } = new();

    // Filtrelenmiş listeler
    public IEnumerable<NotificationVm> UnreadNotifications => AllNotifications.Where(n => !n.IsRead);
    public IEnumerable<NotificationVm> LowStockNotifications => AllNotifications.Where(n => n.Type == 1);
    public IEnumerable<NotificationVm> NewOrderNotifications => AllNotifications.Where(n => n.Type == 2);
    public IEnumerable<NotificationVm> ReturnRequestNotifications => AllNotifications.Where(n => n.Type == 3);
    public IEnumerable<NotificationVm> PaymentFailedNotifications => AllNotifications.Where(n => n.Type == 4);
    public IEnumerable<NotificationVm> CriticalNotifications => AllNotifications.Where(n => n.Priority == 4);
}
