using ECommerce.Application.DTOs;
using ECommerce.Application.DTOs.Dashboard;
using ECommerce.Domain.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ECommerce.Application.Interfaces
{
    public interface INotificationService
    {
        Task<NotificationDto?> GetByIdAsync(int id);
        Task<IReadOnlyList<NotificationDto>> GetAllAsync();
        Task<IReadOnlyList<NotificationDto>> GetUnreadAsync();
        Task<IReadOnlyList<NotificationDto>> GetByTypeAsync(NotificationType type);
        Task<NotificationSummaryDto> GetSummaryAsync();
        Task<IReadOnlyList<LowStockProductDto>> GetLowStockProductsAsync(int threshold = 10);
        Task<IReadOnlyList<NewOrderNotificationDto>> GetRecentOrdersAsync(int count = 10);
        Task<NotificationDto> CreateAsync(NotificationCreateDto dto);
        Task CreateLowStockAlertAsync(int productId, string productName, int currentStock);
        Task CreateNewOrderNotificationAsync(int orderId, string customerName, decimal totalAmount);
        Task CreateReturnRequestNotificationAsync(int orderId, string productName, string customerName, string returnReason);
        Task CreatePaymentFailedNotificationAsync(int orderId, string customerName, string paymentMethod, string errorMessage);
        Task MarkAsReadAsync(int id);
        Task MarkAllAsReadAsync();
        Task DeleteAsync(int id);
        Task CheckAndCreateLowStockAlertsAsync(int threshold = 10);
        Task CleanupOldNotificationsAsync(int daysOld = 30);
    }
}
