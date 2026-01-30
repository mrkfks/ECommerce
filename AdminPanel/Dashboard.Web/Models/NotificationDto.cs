using ECommerce.Domain.Enums;

namespace Dashboard.Web.Models;

/// <summary>
/// Bildirim bilgisi DTO (Dashboard için kopya)
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

    public string TypeIcon => TypeText switch
    {
        "Stok" => "fa-box-open",
        "Sipariş" => "fa-shopping-cart",
        "İade" => "fa-undo",
        "Ödeme" => "fa-credit-card",
        "Kargo" => "fa-truck",
        _ => "fa-bell"
    };

    public string TypeColor => TypeText switch
    {
        "Stok" => "danger",
        "Sipariş" => "success",
        "İade" => "warning",
        "Ödeme" => "danger",
        "Kargo" => "info",
        _ => "secondary"
    };
}
