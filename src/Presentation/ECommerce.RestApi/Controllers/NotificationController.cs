using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;
using ECommerce.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.RestApi.Controllers;

/// <summary>
/// Bildirim yönetimi controller'ı
/// </summary>
[ApiController]
[Route("api/notifications")]
[Authorize]
public class NotificationController : ControllerBase
{
    private readonly INotificationService _notificationService;
    private readonly ILogger<NotificationController> _logger;

    public NotificationController(INotificationService notificationService, ILogger<NotificationController> logger)
    {
        _notificationService = notificationService;
        _logger = logger;
    }

    /// <summary>
    /// Tüm bildirimleri getirir
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<NotificationDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<NotificationDto>>> GetAll()
    {
        var notifications = await _notificationService.GetAllAsync();
        return Ok(notifications);
    }

    /// <summary>
    /// Bildirim özeti getirir
    /// </summary>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(NotificationSummaryDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<NotificationSummaryDto>> GetSummary()
    {
        var summary = await _notificationService.GetSummaryAsync();
        return Ok(summary);
    }

    /// <summary>
    /// Okunmamış bildirimleri getirir
    /// </summary>
    [HttpGet("unread")]
    [ProducesResponseType(typeof(IReadOnlyList<NotificationDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<NotificationDto>>> GetUnread()
    {
        var notifications = await _notificationService.GetUnreadAsync();
        return Ok(notifications);
    }

    /// <summary>
    /// Belirli bir bildirimi getirir
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(NotificationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<NotificationDto>> GetById(int id)
    {
        var notification = await _notificationService.GetByIdAsync(id);
        if (notification == null)
            return NotFound();

        return Ok(notification);
    }

    /// <summary>
    /// Türe göre bildirimleri getirir
    /// </summary>
    [HttpGet("by-type/{type}")]
    [ProducesResponseType(typeof(IReadOnlyList<NotificationDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<NotificationDto>>> GetByType(NotificationType type)
    {
        var notifications = await _notificationService.GetByTypeAsync(type);
        return Ok(notifications);
    }

    /// <summary>
    /// Düşük stoklu ürünleri getirir
    /// </summary>
    [HttpGet("low-stock")]
    [ProducesResponseType(typeof(IReadOnlyList<LowStockProductDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<LowStockProductDto>>> GetLowStockProducts([FromQuery] int threshold = 10)
    {
        var products = await _notificationService.GetLowStockProductsAsync(threshold);
        return Ok(products);
    }

    /// <summary>
    /// Son siparişleri getirir
    /// </summary>
    [HttpGet("recent-orders")]
    [ProducesResponseType(typeof(IReadOnlyList<NewOrderNotificationDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<NewOrderNotificationDto>>> GetRecentOrders([FromQuery] int count = 10)
    {
        var orders = await _notificationService.GetRecentOrdersAsync(count);
        return Ok(orders);
    }

    /// <summary>
    /// Yeni bildirim oluşturur
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(NotificationDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<NotificationDto>> Create([FromBody] NotificationCreateDto dto)
    {
        var notification = await _notificationService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = notification.Id }, notification);
    }

    /// <summary>
    /// Bildirimi okundu olarak işaretler
    /// </summary>
    [HttpPut("{id}/read")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> MarkAsRead(int id)
    {
        await _notificationService.MarkAsReadAsync(id);
        return NoContent();
    }

    /// <summary>
    /// Tüm bildirimleri okundu olarak işaretler
    /// </summary>
    [HttpPut("read-all")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> MarkAllAsRead()
    {
        await _notificationService.MarkAllAsReadAsync();
        return NoContent();
    }

    /// <summary>
    /// Bildirimi siler
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> Delete(int id)
    {
        await _notificationService.DeleteAsync(id);
        return NoContent();
    }

    /// <summary>
    /// Düşük stok kontrolü yapar ve bildirim oluşturur
    /// </summary>
    [HttpPost("check-low-stock")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> CheckLowStock([FromQuery] int threshold = 10)
    {
        await _notificationService.CheckAndCreateLowStockAlertsAsync(threshold);
        return NoContent();
    }

    /// <summary>
    /// Eski bildirimleri temizler
    /// </summary>
    [HttpPost("cleanup")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> CleanupOldNotifications([FromQuery] int daysOld = 30)
    {
        await _notificationService.CleanupOldNotificationsAsync(daysOld);
        return NoContent();
    }
}
