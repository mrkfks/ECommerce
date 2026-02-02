using ECommerce.Application.DTOs;
using ECommerce.Domain.Enums;

namespace Dashboard.Web.Helpers;

public static class NotificationExtensions
{
    public static string TypeColor(this NotificationDto notification)
    {
        return notification.Type switch
        {
            NotificationType.LowStock => "warning",
            NotificationType.NewOrder => "success",
            NotificationType.ReturnRequest => "info",
            NotificationType.PaymentFailed => "danger",
            _ => "info"
        };
    }

    public static string TypeIcon(this NotificationDto notification)
    {
        return notification.Type switch
        {
            NotificationType.LowStock => "fa-triangle-exclamation",
            NotificationType.NewOrder => "fa-box",
            NotificationType.ReturnRequest => "fa-rotate-left",
            NotificationType.PaymentFailed => "fa-credit-card",
            _ => "fa-bell"
        };
    }
}
