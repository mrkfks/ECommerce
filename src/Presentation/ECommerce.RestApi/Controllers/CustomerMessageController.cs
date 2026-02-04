using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.RestApi.Controllers;

[ApiController]
[Route("api/customer-messages")]
public class CustomerMessageController : ControllerBase
{
    private readonly ICustomerMessageService _messageService;
    private readonly ITenantService _tenantService;

    public CustomerMessageController(ICustomerMessageService messageService, ITenantService tenantService)
    {
        _messageService = messageService;
        _tenantService = tenantService;
    }

    /// <summary>
    /// Şirket için tüm mesajları getirir (Admin)
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "SameCompanyOrSuperAdmin")]
    public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _messageService.GetPagedAsync(pageNumber, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// Okunmamış mesajları getirir (Admin)
    /// </summary>
    [HttpGet("unread")]
    [Authorize(Policy = "SameCompanyOrSuperAdmin")]
    public async Task<IActionResult> GetUnread()
    {
        var companyId = _tenantService.GetCompanyId();
        if (!companyId.HasValue) return BadRequest("Şirket bilgisi eksik.");

        var messages = await _messageService.GetUnreadMessagesAsync(companyId.Value);
        return Ok(messages);
    }

    /// <summary>
    /// Kullanıcının kendi mesajlarını getirir
    /// </summary>
    [HttpGet("my")]
    [Authorize]
    public async Task<IActionResult> GetMyMessages()
    {
        var customerId = GetCustomerIdFromClaims();
        if (customerId == null) return Unauthorized();

        var messages = await _messageService.GetMyMessagesAsync(customerId.Value);
        return Ok(messages);
    }

    /// <summary>
    /// Okunmamış mesaj sayısını getirir
    /// </summary>
    [HttpGet("unread-count")]
    [Authorize]
    public async Task<IActionResult> GetUnreadCount()
    {
        var customerId = GetCustomerIdFromClaims();
        if (customerId == null) return Unauthorized();

        var count = await _messageService.GetUnreadCountAsync(customerId.Value);
        return Ok(new { count });
    }

    /// <summary>
    /// Belirli bir mesajı getirir
    /// </summary>
    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetById(int id)
    {
        var message = await _messageService.GetByIdAsync(id);
        if (message == null)
            return NotFound(new { message = "Mesaj bulunamadı." });

        return Ok(message);
    }

    /// <summary>
    /// Yeni mesaj gönderir
    /// </summary>
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] CustomerMessageFormDto dto)
    {
        var customerId = GetCustomerIdFromClaims();
        if (customerId == null) return Unauthorized();

        try
        {
            var message = await _messageService.CreateMessageAsync(dto, customerId.Value);
            return Ok(new { success = true, data = message, message = "Mesajınız başarıyla gönderildi." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Mesajı okundu olarak işaretler
    /// </summary>
    [HttpPut("{id}/read")]
    [Authorize]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        try
        {
            await _messageService.MarkAsReadAsync(id);
            return Ok(new { message = "Mesaj okundu olarak işaretlendi." });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Mesaja yanıt verir (Admin)
    /// </summary>
    [HttpPost("{id}/reply")]
    [Authorize(Policy = "SameCompanyOrSuperAdmin")]
    public async Task<IActionResult> Reply(int id, [FromBody] CustomerMessageReplyDto dto)
    {
        var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "userId")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            return Unauthorized();

        try
        {
            await _messageService.ReplyToMessageAsync(id, dto.Reply, userId);
            return Ok(new { message = "Yanıt başarıyla gönderildi." });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    private int? GetCustomerIdFromClaims()
    {
        var customerIdClaim = User.Claims.FirstOrDefault(c => c.Type == "customerId")?.Value;
        if (!string.IsNullOrEmpty(customerIdClaim) && int.TryParse(customerIdClaim, out int customerId))
        {
            return customerId;
        }
        return null;
    }
}

