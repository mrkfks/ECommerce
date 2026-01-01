using ECommerce.Application.DTOs;
using ECommerce.Domain.Enums;

namespace ECommerce.Application.Interfaces;

/// <summary>
/// Bildirim servisi interface'i
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Bildirim ID'sine göre getirir
    /// </summary>
    Task<NotificationDto?> GetByIdAsync(int id);

    /// <summary>
    /// Tüm bildirimleri getirir
    /// </summary>
    Task<IReadOnlyList<NotificationDto>> GetAllAsync();

    /// <summary>
    /// Okunmamış bildirimleri getirir
    /// </summary>
    Task<IReadOnlyList<NotificationDto>> GetUnreadAsync();

    /// <summary>
    /// Bildirim türüne göre getirir
    /// </summary>
    Task<IReadOnlyList<NotificationDto>> GetByTypeAsync(NotificationType type);

    /// <summary>
    /// Bildirim özeti getirir
    /// </summary>
    Task<NotificationSummaryDto> GetSummaryAsync();

    /// <summary>
    /// Düşük stoklu ürünleri getirir
    /// </summary>
    Task<IReadOnlyList<LowStockProductDto>> GetLowStockProductsAsync(int threshold = 10);

    /// <summary>
    /// Son siparişleri getirir (bildirimsiz)
    /// </summary>
    Task<IReadOnlyList<NewOrderNotificationDto>> GetRecentOrdersAsync(int count = 10);

    /// <summary>
    /// Yeni bildirim oluşturur
    /// </summary>
    Task<NotificationDto> CreateAsync(NotificationCreateDto dto);

    /// <summary>
    /// Düşük stok bildirimi oluşturur
    /// </summary>
    Task CreateLowStockAlertAsync(int productId, string productName, int currentStock);

    /// <summary>
    /// Yeni sipariş bildirimi oluşturur
    /// </summary>
    Task CreateNewOrderNotificationAsync(int orderId, string customerName, decimal totalAmount);

    /// <summary>
    /// İade talebi bildirimi oluşturur
    /// </summary>
    Task CreateReturnRequestNotificationAsync(int orderId, string productName, string customerName, string returnReason);

    /// <summary>
    /// Başarısız ödeme bildirimi oluşturur
    /// </summary>
    Task CreatePaymentFailedNotificationAsync(int orderId, string customerName, string paymentMethod, string errorMessage);

    /// <summary>
    /// Bildirimi okundu olarak işaretle
    /// </summary>
    Task MarkAsReadAsync(int id);

    /// <summary>
    /// Tüm bildirimleri okundu olarak işaretle
    /// </summary>
    Task MarkAllAsReadAsync();

    /// <summary>
    /// Bildirimi sil
    /// </summary>
    Task DeleteAsync(int id);

    /// <summary>
    /// Stok kontrolü yap ve gerekirse bildirim oluştur
    /// </summary>
    Task CheckAndCreateLowStockAlertsAsync(int threshold = 10);

    /// <summary>
    /// Eski bildirimleri temizle
    /// </summary>
    Task CleanupOldNotificationsAsync(int daysOld = 30);
}
