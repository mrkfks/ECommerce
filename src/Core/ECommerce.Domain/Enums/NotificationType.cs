namespace ECommerce.Domain.Enums;

/// <summary>
/// Bildirim türleri
/// </summary>
public enum NotificationType
{
    /// <summary>
    /// Düşük stok uyarısı
    /// </summary>
    LowStock = 1,

    /// <summary>
    /// Yeni sipariş
    /// </summary>
    NewOrder = 2,

    /// <summary>
    /// İade talebi
    /// </summary>
    ReturnRequest = 3,

    /// <summary>
    /// Başarısız ödeme
    /// </summary>
    PaymentFailed = 4,

    /// <summary>
    /// Sipariş durumu değişikliği
    /// </summary>
    OrderStatusChanged = 5,

    /// <summary>
    /// Sistem bildirimi
    /// </summary>
    System = 6
}

/// <summary>
/// Bildirim öncelik seviyeleri
/// </summary>
public enum NotificationPriority
{
    /// <summary>
    /// Düşük öncelik - Bilgilendirme
    /// </summary>
    Low = 1,

    /// <summary>
    /// Normal öncelik
    /// </summary>
    Normal = 2,

    /// <summary>
    /// Yüksek öncelik - Uyarı
    /// </summary>
    High = 3,

    /// <summary>
    /// Kritik - Acil müdahale gerekli
    /// </summary>
    Critical = 4
}
