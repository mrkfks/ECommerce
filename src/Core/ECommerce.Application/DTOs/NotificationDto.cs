using ECommerce.Domain.Enums;

namespace ECommerce.Application.DTOs;

/// <summary>
/// Bildirim DTO'su
/// </summary>
public class NotificationDto
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public NotificationType Type { get; set; }
    public string TypeText { get; set; } = string.Empty;
    public NotificationPriority Priority { get; set; }
    public string PriorityText { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? EntityType { get; set; }
    public int? EntityId { get; set; }
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
    public string? Data { get; set; }
    public string? ActionUrl { get; set; }
    public string? ActionText { get; set; }
    public DateTime CreatedAt { get; set; }
    public string TimeAgo { get; set; } = string.Empty;
}

/// <summary>
/// Bildirim listesi özet DTO'su
/// </summary>
public class NotificationSummaryDto
{
    public int TotalCount { get; set; }
    public int UnreadCount { get; set; }
    public int LowStockCount { get; set; }
    public int NewOrderCount { get; set; }
    public int ReturnRequestCount { get; set; }
    public int PaymentFailedCount { get; set; }
    public List<NotificationDto> RecentNotifications { get; set; } = new();
}

/// <summary>
/// Bildirim oluşturma DTO'su
/// </summary>
public class NotificationCreateDto
{
    public NotificationType Type { get; set; }
    public NotificationPriority Priority { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? EntityType { get; set; }
    public int? EntityId { get; set; }
    public string? Data { get; set; }
    public string? ActionUrl { get; set; }
    public string? ActionText { get; set; }
}

/// <summary>
/// Düşük stok ürün DTO'su
/// </summary>
public class LowStockProductDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public int CurrentStock { get; set; }
    public int Threshold { get; set; } = 10;
    public string CategoryName { get; set; } = string.Empty;
    public string BrandName { get; set; } = string.Empty;
    public decimal Price { get; set; }
}

/// <summary>
/// Yeni sipariş bildirim DTO'su
/// </summary>
public class NewOrderNotificationDto
{
    public int OrderId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public int ItemCount { get; set; }
    public DateTime OrderDate { get; set; }
    public string TimeAgo { get; set; } = string.Empty;
}

/// <summary>
/// İade talebi DTO'su
/// </summary>
public class ReturnRequestDto
{
    public int OrderId { get; set; }
    public int OrderItemId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string ReturnReason { get; set; } = string.Empty;
    public DateTime RequestDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal RefundAmount { get; set; }
}

/// <summary>
/// Başarısız ödeme DTO'su
/// </summary>
public class FailedPaymentDto
{
    public int OrderId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime AttemptDate { get; set; }
    public int RetryCount { get; set; }
}
