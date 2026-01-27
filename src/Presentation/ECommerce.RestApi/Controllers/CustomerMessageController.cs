using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.RestApi.Controllers;

[ApiController]
[Route("api/customer-messages")]
[Authorize(Policy = "SameCompanyOrSuperAdmin")]
public class CustomerMessageController : ControllerBase
{
    private readonly ICustomerMessageService _messageService;

    public CustomerMessageController(ICustomerMessageService messageService)
    {
        _messageService = messageService;
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
        var messages = await _messageService.GetUnreadAsync();
        return Ok(messages);
    }

    [HttpGet("pending")]
    public async Task<IActionResult> GetPending()
    {
        var messages = await _messageService.GetPendingAsync();
        return Ok(messages);
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary()
    {
        var summary = await _messageService.GetSummaryAsync();
        return Ok(summary);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var message = await _messageService.GetByIdAsync(id);
        if (message == null)
            return NotFound(new { message = "Mesaj bulunamadı." });

        return Ok(message);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CustomerMessageCreateDto dto)
    {
        try
        {
            var id = await _messageService.CreateAsync(dto);
            return Ok(new { id, message = "Mesaj oluşturuldu." });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
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

    [HttpPost("{id}/reply")]
    public async Task<IActionResult> Reply(int id, [FromBody] CustomerMessageReplyDto dto)
    {
        try
        {
            await _messageService.SendReplyAsync(id, dto);
            return Ok(new { message = "Yanıt gönderildi." });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _messageService.DeleteAsync(id);
            return Ok(new { message = "Mesaj silindi." });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}
