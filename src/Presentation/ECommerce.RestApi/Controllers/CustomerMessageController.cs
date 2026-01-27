using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.RestApi.Controllers;

[ApiController]
[Route("api/customer-messages")]
[Authorize(Policy = "SameCompanyOrSuperAdmin")]
public class CustomerMessageController : ControllerBase
{
    private readonly ICustomerMessageService _messageService;
    private readonly ITenantService _tenantService;

    public CustomerMessageController(ICustomerMessageService messageService, ITenantService tenantService)
    {
        _messageService = messageService;
        _tenantService = tenantService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _messageService.GetPagedAsync(pageNumber, pageSize);
        return Ok(result);
    }

    [HttpGet("unread")]
    public async Task<IActionResult> GetUnread()
    {
        var companyId = _tenantService.GetCompanyId();
        if (!companyId.HasValue) return BadRequest("Şirket bilgisi eksik.");

        var messages = await _messageService.GetUnreadMessagesAsync(companyId.Value);
        return Ok(messages);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var message = await _messageService.GetByIdAsync(id);
        if (message == null)
            return NotFound(new { message = "Mesaj bulunamadı." });

        return Ok(message);
    }

    [HttpPut("{id}/read")]
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
}
