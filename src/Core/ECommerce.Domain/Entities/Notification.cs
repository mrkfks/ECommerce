using ECommerce.Domain.Enums;

namespace ECommerce.Domain.Entities;

/// <summary>
/// Bildirim entity'si - Tüm bildirim türlerini yönetir
/// </summary>
public class Notification : BaseEntity, ITenantEntity
{
    private Notification() { }

    /// <summary>
    /// Şirket ID'si (Multi-tenant)
    /// </summary>
    public int CompanyId { get; private set; }

    /// <summary>
    /// Bildirim türü
    /// </summary>
    public NotificationType Type { get; private set; }

    /// <summary>
    /// Bildirim önceliği
    /// </summary>
    public NotificationPriority Priority { get; private set; }

    /// <summary>
    /// Bildirim başlığı
    /// </summary>
    public string Title { get; private set; } = string.Empty;

    /// <summary>
    /// Bildirim mesajı
    /// </summary>
    public string Message { get; private set; } = string.Empty;

    /// <summary>
    /// İlgili entity türü (Product, Order, Customer vb.)
    /// </summary>
    public string? EntityType { get; private set; }

    /// <summary>
    /// İlgili entity ID'si
    /// </summary>
    public int? EntityId { get; private set; }

    /// <summary>
    /// Okundu mu?
    /// </summary>
    public bool IsRead { get; private set; }

    /// <summary>
    /// Okunma tarihi
    /// </summary>
    public DateTime? ReadAt { get; private set; }

    /// <summary>
    /// Ek veri (JSON formatında)
    /// </summary>
    public string? Data { get; private set; }

    /// <summary>
    /// Yönlendirme URL'i
    /// </summary>
    public string? ActionUrl { get; private set; }

    /// <summary>
    /// Aksiyon butonu metni
    /// </summary>
    public string? ActionText { get; private set; }

    // Navigation
    public virtual Company? Company { get; private set; }

    /// <summary>
    /// Yeni bildirim oluşturur
    /// </summary>
    public static Notification Create(
        int companyId,
        NotificationType type,
        NotificationPriority priority,
        string title,
        string message,
        string? entityType = null,
        int? entityId = null,
        string? data = null,
        string? actionUrl = null,
        string? actionText = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Bildirim başlığı boş olamaz.", nameof(title));

        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("Bildirim mesajı boş olamaz.", nameof(message));

        return new Notification
        {
            CompanyId = companyId,
            Type = type,
            Priority = priority,
            Title = title,
            Message = message,
            EntityType = entityType,
            EntityId = entityId,
            Data = data,
            ActionUrl = actionUrl,
            ActionText = actionText,
            IsRead = false
        };
    }

    /// <summary>
    /// Düşük stok bildirimi oluşturur
    /// </summary>
    public static Notification CreateLowStockAlert(
        int companyId,
        int productId,
        string productName,
        int currentStock,
        int threshold = 10)
    {
        return Create(
            companyId,
            NotificationType.LowStock,
            NotificationPriority.Critical,
            "Düşük Stok Uyarısı",
            $"'{productName}' ürününün stok seviyesi kritik! Mevcut: {currentStock} adet.",
            "Product",
            productId,
            System.Text.Json.JsonSerializer.Serialize(new { ProductName = productName, CurrentStock = currentStock, Threshold = threshold }),
            $"/Product/Edit/{productId}",
            "Stok Güncelle"
        );
    }

    /// <summary>
    /// Yeni sipariş bildirimi oluşturur
    /// </summary>
    public static Notification CreateNewOrderNotification(
        int companyId,
        int orderId,
        string customerName,
        decimal totalAmount)
    {
        return Create(
            companyId,
            NotificationType.NewOrder,
            NotificationPriority.Normal,
            "Yeni Sipariş",
            $"{customerName} tarafından {totalAmount:C} tutarında yeni sipariş alındı.",
            "Order",
            orderId,
            System.Text.Json.JsonSerializer.Serialize(new { CustomerName = customerName, TotalAmount = totalAmount }),
            $"/Order/Details/{orderId}",
            "Detayları Gör"
        );
    }

    /// <summary>
    /// İade talebi bildirimi oluşturur
    /// </summary>
    public static Notification CreateReturnRequestNotification(
        int companyId,
        int orderId,
        string productName,
        string customerName,
        string returnReason)
    {
        return Create(
            companyId,
            NotificationType.ReturnRequest,
            NotificationPriority.High,
            "İade Talebi",
            $"{customerName} tarafından '{productName}' ürünü için iade talebi oluşturuldu.",
            "Order",
            orderId,
            System.Text.Json.JsonSerializer.Serialize(new { ProductName = productName, CustomerName = customerName, Reason = returnReason }),
            $"/Order/Details/{orderId}",
            "İncele"
        );
    }

    /// <summary>
    /// Başarısız ödeme bildirimi oluşturur
    /// </summary>
    public static Notification CreatePaymentFailedNotification(
        int companyId,
        int orderId,
        string customerName,
        string paymentMethod,
        string errorMessage)
    {
        return Create(
            companyId,
            NotificationType.PaymentFailed,
            NotificationPriority.Critical,
            "Ödeme Hatası",
            $"{customerName} siparişinde ödeme başarısız oldu: {errorMessage}",
            "Order",
            orderId,
            System.Text.Json.JsonSerializer.Serialize(new { CustomerName = customerName, PaymentMethod = paymentMethod, Error = errorMessage }),
            $"/Order/Details/{orderId}",
            "İncele"
        );
    }

    /// <summary>
    /// Bildirimi okundu olarak işaretle
    /// </summary>
    public void MarkAsRead()
    {
        if (IsRead) return;

        IsRead = true;
        ReadAt = DateTime.UtcNow;
        MarkAsModified();
    }

    /// <summary>
    /// Bildirimi okunmadı olarak işaretle
    /// </summary>
    public void MarkAsUnread()
    {
        if (!IsRead) return;

        IsRead = false;
        ReadAt = null;
        MarkAsModified();
    }
}
