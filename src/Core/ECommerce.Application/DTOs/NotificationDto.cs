using ECommerce.Domain.Enums;

namespace ECommerce.Application.DTOs;

/// <summary>
/// Bildirim bilgisi DTO
/// </summary>
public record NotificationDto
{
    public int Id { get; init; }
    public int CompanyId { get; init; }
    public NotificationType Type { get; init; }
    public string? TypeText { get; init; }
    public string Title { get; init; } = string.Empty;
    public NotificationPriority Priority { get; init; }
    public string? PriorityText { get; init; }
    public string Message { get; init; } = string.Empty;
    public string Data { get; init; } = string.Empty;
    public string EntityType { get; init; } = string.Empty;
    public int? EntityId { get; init; }
    public int? UserId { get; init; }
    public bool IsRead { get; init; }
    public DateTime? ReadAt { get; init; }
    public string? ReadBy { get; init; }
    public string? ActionUrl { get; init; }
    public string? ActionText { get; init; }
    public string? Icon { get; init; }
    public DateTime CreatedAt { get; init; }
    public string Severity { get; init; } = "info";
    public string? TimeAgo { get; init; }
}

/// <summary>
/// Bildirim form DTO
/// </summary>
public record NotificationFormDto
{
    public int? Id { get; init; }
    public int CompanyId { get; init; }
    public NotificationType Type { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public NotificationPriority Priority { get; init; } = NotificationPriority.Normal;
    public string? EntityType { get; init; }
    public int? EntityId { get; init; }
    public string? Data { get; init; }
    public string? ActionUrl { get; init; }
    public string? ActionText { get; init; }
}

/// <summary>
/// Bildirim özet DTO
/// </summary>
public record NotificationSummaryDto
{
    public int TotalCount { get; init; }
    public int UnreadCount { get; init; }
    public int HighPriorityCount { get; init; }
    public int LowStockCount { get; init; }
    public int NewOrderCount { get; init; }
    public int ReturnRequestCount { get; init; }
    public int PaymentFailedCount { get; init; }
    public int SystemAlertCount { get; init; }
    public List<NotificationDto> RecentNotifications { get; init; } = new();
}

/// <summary>
/// Yeni sipariş bildirimi DTO
/// </summary>
public record NewOrderNotificationDto
{
    public int OrderId { get; init; }
    public string CustomerName { get; init; } = string.Empty;
    public decimal TotalAmount { get; init; }
    public int ItemCount { get; init; }
    public DateTime OrderDate { get; init; }
    public string Status { get; init; } = string.Empty;
    public string? TimeAgo { get; init; }
}
