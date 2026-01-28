using AutoMapper;
using ECommerce.Application.DTOs;
using ECommerce.Application.DTOs.Dashboard;
using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Services;

/// <summary>
/// Bildirim servisi implementasyonu
/// </summary>
public class NotificationService : INotificationService
{
    private readonly AppDbContext _context;
    private readonly ITenantService _tenantService;

    public NotificationService(AppDbContext context, ITenantService tenantService)
    {
        _context = context;
        _tenantService = tenantService;
    }

    public async Task<NotificationDto?> GetByIdAsync(int id)
    {
        var notification = await _context.Notifications.FindAsync(id);
        return notification != null ? MapToDto(notification) : null;
    }

    public async Task<IReadOnlyList<NotificationDto>> GetAllAsync()
    {
        var notifications = await _context.Notifications
            .OrderByDescending(n => n.CreatedAt)
            .Take(100)
            .ToListAsync();

        return notifications.Select(MapToDto).ToList();
    }

    public async Task<IReadOnlyList<NotificationDto>> GetUnreadAsync()
    {
        var notifications = await _context.Notifications
            .Where(n => !n.IsRead)
            .OrderByDescending(n => n.CreatedAt)
            .Take(50)
            .ToListAsync();

        return notifications.Select(MapToDto).ToList();
    }

    public async Task<IReadOnlyList<NotificationDto>> GetByTypeAsync(NotificationType type)
    {
        var notifications = await _context.Notifications
            .Where(n => n.Type == type)
            .OrderByDescending(n => n.CreatedAt)
            .Take(50)
            .ToListAsync();

        return notifications.Select(MapToDto).ToList();
    }

    public async Task<NotificationSummaryDto> GetSummaryAsync()
    {
        var allNotifications = await _context.Notifications
            .OrderByDescending(n => n.CreatedAt)
            .Take(100)
            .ToListAsync();

        var recentUnread = allNotifications
            .Where(n => !n.IsRead)
            .Take(10)
            .ToList();

        return new NotificationSummaryDto
        {
            TotalCount = allNotifications.Count,
            UnreadCount = allNotifications.Count(n => !n.IsRead),
            LowStockCount = allNotifications.Count(n => n.Type == NotificationType.LowStock && !n.IsRead),
            NewOrderCount = allNotifications.Count(n => n.Type == NotificationType.NewOrder && !n.IsRead),
            ReturnRequestCount = allNotifications.Count(n => n.Type == NotificationType.ReturnRequest && !n.IsRead),
            PaymentFailedCount = allNotifications.Count(n => n.Type == NotificationType.PaymentFailed && !n.IsRead),
            RecentNotifications = recentUnread.Select(MapToDto).ToList()
        };
    }

    public async Task<IReadOnlyList<LowStockProductDto>> GetLowStockProductsAsync(int threshold = 10)
    {
        var products = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .Where(p => p.StockQuantity <= threshold && p.IsActive)
            .OrderBy(p => p.StockQuantity)
            .Take(50)
            .ToListAsync();

        return products.Select(p => new LowStockProductDto
        {
            ProductId = p.Id,
            ProductName = p.Name,
            ImageUrl = p.ImageUrl,
            CurrentStock = p.StockQuantity,
            Threshold = threshold,
            CategoryName = p.Category?.Name ?? "Bilinmiyor",
            BrandName = p.Brand?.Name ?? "Bilinmiyor",
            Price = p.Price
        }).ToList();
    }

    public async Task<IReadOnlyList<NewOrderNotificationDto>> GetRecentOrdersAsync(int count = 10)
    {
        var orders = await _context.Orders
            .Include(o => o.Customer)
            .Include(o => o.Items)
            .Where(o => o.Status == OrderStatus.Pending)
            .OrderByDescending(o => o.OrderDate)
            .Take(count)
            .ToListAsync();

        return orders.Select(o => new NewOrderNotificationDto
        {
            OrderId = o.Id,
            CustomerName = o.Customer != null ? $"{o.Customer.FirstName} {o.Customer.LastName}" : "Bilinmiyor",
            TotalAmount = o.TotalAmount,
            ItemCount = o.Items.Count,
            OrderDate = o.OrderDate,
            TimeAgo = GetTimeAgo(o.OrderDate)
        }).ToList();
    }

    public async Task<NotificationDto> CreateAsync(NotificationCreateDto dto)
    {
        var companyId = _tenantService.GetCompanyId() ?? 1;

        var notification = Notification.Create(
            companyId,
            dto.Type,
            dto.Priority,
            dto.Title,
            dto.Message,
            dto.EntityType,
            dto.EntityId,
            dto.Data,
            dto.ActionUrl,
            dto.ActionText
        );

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();

        return MapToDto(notification);
    }

    public async Task CreateLowStockAlertAsync(int productId, string productName, int currentStock)
    {
        var companyId = _tenantService.GetCompanyId() ?? 1;

        // Aynı ürün için son 24 saatte bildirim var mı kontrol et
        var existingNotification = await _context.Notifications
            .Where(n => n.Type == NotificationType.LowStock
                     && n.EntityId == productId
                     && n.CreatedAt > DateTime.UtcNow.AddHours(-24))
            .FirstOrDefaultAsync();

        if (existingNotification != null) return;

        var notification = Notification.CreateLowStockAlert(companyId, productId, productName, currentStock);
        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();
    }

    public async Task CreateNewOrderNotificationAsync(int orderId, string customerName, decimal totalAmount)
    {
        var companyId = _tenantService.GetCompanyId() ?? 1;

        var notification = Notification.CreateNewOrderNotification(companyId, orderId, customerName, totalAmount);
        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();
    }

    public async Task CreateReturnRequestNotificationAsync(int orderId, string productName, string customerName, string returnReason)
    {
        var companyId = _tenantService.GetCompanyId() ?? 1;

        var notification = Notification.CreateReturnRequestNotification(companyId, orderId, productName, customerName, returnReason);
        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();
    }

    public async Task CreatePaymentFailedNotificationAsync(int orderId, string customerName, string paymentMethod, string errorMessage)
    {
        var companyId = _tenantService.GetCompanyId() ?? 1;

        var notification = Notification.CreatePaymentFailedNotification(companyId, orderId, customerName, paymentMethod, errorMessage);
        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();
    }

    public async Task MarkAsReadAsync(int id)
    {
        var notification = await _context.Notifications.FindAsync(id);
        if (notification != null)
        {
            notification.MarkAsRead();
            await _context.SaveChangesAsync();
        }
    }

    public async Task MarkAllAsReadAsync()
    {
        var unreadNotifications = await _context.Notifications
            .Where(n => !n.IsRead)
            .ToListAsync();

        foreach (var notification in unreadNotifications)
        {
            notification.MarkAsRead();
        }

        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var notification = await _context.Notifications.FindAsync(id);
        if (notification != null)
        {
            notification.Delete();
            await _context.SaveChangesAsync();
        }
    }

    public async Task CheckAndCreateLowStockAlertsAsync(int threshold = 10)
    {
        var lowStockProducts = await _context.Products
            .Where(p => p.StockQuantity <= threshold && p.IsActive)
            .ToListAsync();

        foreach (var product in lowStockProducts)
        {
            await CreateLowStockAlertAsync(product.Id, product.Name, product.StockQuantity);
        }
    }

    public async Task CleanupOldNotificationsAsync(int daysOld = 30)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-daysOld);

        var oldNotifications = await _context.Notifications
            .Where(n => n.CreatedAt < cutoffDate && n.IsRead)
            .ToListAsync();

        foreach (var notification in oldNotifications)
        {
            notification.Delete();
        }

        await _context.SaveChangesAsync();
    }

    private NotificationDto MapToDto(Notification notification)
    {
        return new NotificationDto
        {
            Id = notification.Id,
            CompanyId = notification.CompanyId,
            Type = notification.Type,
            TypeText = GetTypeText(notification.Type),
            Priority = notification.Priority,
            PriorityText = GetPriorityText(notification.Priority),
            Title = notification.Title,
            Message = notification.Message,
            EntityType = notification.EntityType,
            EntityId = notification.EntityId,
            IsRead = notification.IsRead,
            ReadAt = notification.ReadAt,
            Data = notification.Data,
            ActionUrl = notification.ActionUrl,
            ActionText = notification.ActionText,
            CreatedAt = notification.CreatedAt,
            TimeAgo = GetTimeAgo(notification.CreatedAt)
        };
    }

    private static string GetTypeText(NotificationType type)
    {
        return type switch
        {
            NotificationType.LowStock => "Düşük Stok",
            NotificationType.NewOrder => "Yeni Sipariş",
            NotificationType.ReturnRequest => "İade Talebi",
            NotificationType.PaymentFailed => "Ödeme Hatası",
            NotificationType.OrderStatusChanged => "Sipariş Durumu",
            NotificationType.System => "Sistem",
            _ => "Diğer"
        };
    }

    private static string GetPriorityText(NotificationPriority priority)
    {
        return priority switch
        {
            NotificationPriority.Low => "Düşük",
            NotificationPriority.Normal => "Normal",
            NotificationPriority.High => "Yüksek",
            NotificationPriority.Critical => "Kritik",
            _ => "Bilinmiyor"
        };
    }

    private static string GetTimeAgo(DateTime dateTime)
    {
        var timeSpan = DateTime.UtcNow - dateTime;

        if (timeSpan.TotalMinutes < 1) return "Az önce";
        if (timeSpan.TotalMinutes < 60) return $"{(int)timeSpan.TotalMinutes} dakika önce";
        if (timeSpan.TotalHours < 24) return $"{(int)timeSpan.TotalHours} saat önce";
        if (timeSpan.TotalDays < 7) return $"{(int)timeSpan.TotalDays} gün önce";
        if (timeSpan.TotalDays < 30) return $"{(int)(timeSpan.TotalDays / 7)} hafta önce";

        return dateTime.ToString("dd MMM yyyy");
    }
}
