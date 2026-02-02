using Dashboard.Web.Services;
using ECommerce.Domain.Enums;

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
    public List<ECommerce.Application.DTOs.NotificationDto> AllNotifications { get; set; } = new();

    /// <summary>
    /// Düşük stoklu ürünler
    /// </summary>
    public List<LowStockItemVm> LowStockProducts { get; set; } = new();

    /// <summary>
    /// Son siparişler
    /// </summary>
    public List<RecentOrderVm> RecentOrders { get; set; } = new();

    // Filtrelenmiş listeler
    public IEnumerable<ECommerce.Application.DTOs.NotificationDto> UnreadNotifications => AllNotifications.Where(n => !n.IsRead);
    public IEnumerable<ECommerce.Application.DTOs.NotificationDto> LowStockNotifications => AllNotifications.Where(n => n.Type == NotificationType.LowStock);
    public IEnumerable<ECommerce.Application.DTOs.NotificationDto> NewOrderNotifications => AllNotifications.Where(n => n.Type == NotificationType.NewOrder);
    public IEnumerable<ECommerce.Application.DTOs.NotificationDto> ReturnRequestNotifications => AllNotifications.Where(n => n.Type == NotificationType.ReturnRequest);
    public IEnumerable<ECommerce.Application.DTOs.NotificationDto> PaymentFailedNotifications => AllNotifications.Where(n => n.Type == NotificationType.PaymentFailed);
    public IEnumerable<ECommerce.Application.DTOs.NotificationDto> CriticalNotifications => AllNotifications.Where(n => n.Priority == NotificationPriority.Critical);
}
