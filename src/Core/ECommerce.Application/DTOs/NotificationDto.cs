using ECommerce.Domain.Enums;

namespace ECommerce.Application.DTOs;

/// <summary>
/// Bildirim DTO'su
/// </summary>
public record NotificationDto(
    int Id,
    int CompanyId,
    NotificationType Type,
    string TypeText,
    NotificationPriority Priority,
    string PriorityText,
    string Title,
    string Message,
    string? EntityType,
    int? EntityId,
    bool IsRead,
    DateTime? ReadAt,
    string? Data,
    string? ActionUrl,
    string? ActionText,
    DateTime CreatedAt,
    string TimeAgo
);

/// <summary>
/// Bildirim listesi özet DTO'su
/// </summary>
public record NotificationSummaryDto(
    int TotalCount,
    int UnreadCount,
    int LowStockCount,
    int NewOrderCount,
    int ReturnRequestCount,
    int PaymentFailedCount,
    List<NotificationDto> RecentNotifications
);

/// <summary>
/// Bildirim oluşturma DTO'su
/// </summary>
public record NotificationCreateDto(
    NotificationType Type,
    NotificationPriority Priority,
    string Title,
    string Message,
    string? EntityType = null,
    int? EntityId = null,
    string? Data = null,
    string? ActionUrl = null,
    string? ActionText = null
);

/// <summary>
/// Düşük stok ürün DTO'su
/// </summary>
public record LowStockProductDto(
    int ProductId,
    string ProductName,
    string? ImageUrl,
    int CurrentStock,
    int Threshold = 10,
    string CategoryName = "",
    string BrandName = "",
    decimal Price = 0
);

/// <summary>
/// Yeni sipariş bildirim DTO'su
/// </summary>
public record NewOrderNotificationDto(
    int OrderId,
    string CustomerName,
    decimal TotalAmount,
    int ItemCount,
    DateTime OrderDate,
    string TimeAgo
);

/// <summary>
/// İade talebi DTO'su
/// </summary>
public record ReturnRequestDto(
    int OrderId,
    int OrderItemId,
    string ProductName,
    string CustomerName,
    string ReturnReason,
    DateTime RequestDate,
    string Status,
    decimal RefundAmount
);

/// <summary>
/// Başarısız ödeme DTO'su
/// </summary>
public record FailedPaymentDto(
    int OrderId,
    string CustomerName,
    string PaymentMethod,
    string ErrorMessage,
    decimal Amount,
    DateTime AttemptDate,
    int RetryCount
);
