using ECommerce.Application.DTOs;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.RestApi.Controllers;

[ApiController]
[Route("api/customer-messages")]
[Authorize(Policy = "SameCompanyOrSuperAdmin")]
public class CustomerMessageController : ControllerBase
{
    private readonly AppDbContext _context;

    public CustomerMessageController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var messages = await _context.CustomerMessages
            .Include(m => m.Customer)
            .Include(m => m.RepliedBy)
            .AsNoTracking()
            .Select(m => new CustomerMessageDto
            {
                Id = m.Id,
                CustomerId = m.CustomerId,
                CustomerName = m.Customer != null ? m.Customer.FirstName + " " + m.Customer.LastName : "",
                CustomerEmail = m.Customer != null ? m.Customer.Email : "",
                CompanyId = m.CompanyId,
                Subject = m.Subject,
                Message = m.Message,
                IsRead = m.IsRead,
                IsReplied = m.IsReplied,
                Reply = m.Reply,
                RepliedAt = m.RepliedAt,
                RepliedByUserId = m.RepliedByUserId,
                RepliedByName = m.RepliedBy != null ? m.RepliedBy.FirstName + " " + m.RepliedBy.LastName : null,
                Category = m.Category,
                CategoryText = GetCategoryText(m.Category),
                CreatedAt = m.CreatedAt
            })
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync();

        return Ok(messages);
    }

    [HttpGet("unread")]
    public async Task<IActionResult> GetUnread()
    {
        var messages = await _context.CustomerMessages
            .Include(m => m.Customer)
            .Where(m => !m.IsRead)
            .AsNoTracking()
            .Select(m => new CustomerMessageDto
            {
                Id = m.Id,
                CustomerId = m.CustomerId,
                CustomerName = m.Customer != null ? m.Customer.FirstName + " " + m.Customer.LastName : "",
                CustomerEmail = m.Customer != null ? m.Customer.Email : "",
                CompanyId = m.CompanyId,
                Subject = m.Subject,
                Message = m.Message,
                IsRead = m.IsRead,
                IsReplied = m.IsReplied,
                Category = m.Category,
                CategoryText = GetCategoryText(m.Category),
                CreatedAt = m.CreatedAt
            })
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync();

        return Ok(messages);
    }

    [HttpGet("pending")]
    public async Task<IActionResult> GetPending()
    {
        var messages = await _context.CustomerMessages
            .Include(m => m.Customer)
            .Where(m => !m.IsReplied)
            .AsNoTracking()
            .Select(m => new CustomerMessageDto
            {
                Id = m.Id,
                CustomerId = m.CustomerId,
                CustomerName = m.Customer != null ? m.Customer.FirstName + " " + m.Customer.LastName : "",
                CustomerEmail = m.Customer != null ? m.Customer.Email : "",
                CompanyId = m.CompanyId,
                Subject = m.Subject,
                Message = m.Message,
                IsRead = m.IsRead,
                IsReplied = m.IsReplied,
                Category = m.Category,
                CategoryText = GetCategoryText(m.Category),
                CreatedAt = m.CreatedAt
            })
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync();

        return Ok(messages);
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary()
    {
        var messages = await _context.CustomerMessages
            .Include(m => m.Customer)
            .AsNoTracking()
            .ToListAsync();

        var summary = new CustomerMessageSummaryDto
        {
            TotalMessages = messages.Count,
            UnreadMessages = messages.Count(m => !m.IsRead),
            PendingReplies = messages.Count(m => !m.IsReplied),
            RepliedMessages = messages.Count(m => m.IsReplied),
            RecentMessages = messages
                .OrderByDescending(m => m.CreatedAt)
                .Take(10)
                .Select(m => new CustomerMessageDto
                {
                    Id = m.Id,
                    CustomerId = m.CustomerId,
                    CustomerName = m.Customer != null ? m.Customer.FirstName + " " + m.Customer.LastName : "",
                    CustomerEmail = m.Customer?.Email ?? "",
                    CompanyId = m.CompanyId,
                    Subject = m.Subject,
                    Message = m.Message,
                    IsRead = m.IsRead,
                    IsReplied = m.IsReplied,
                    Category = m.Category,
                    CategoryText = GetCategoryText(m.Category),
                    CreatedAt = m.CreatedAt
                })
                .ToList()
        };

        return Ok(summary);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var message = await _context.CustomerMessages
            .Include(m => m.Customer)
            .Include(m => m.RepliedBy)
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id);

        if (message == null)
            return NotFound(new { message = "Mesaj bulunamadı." });

        return Ok(new CustomerMessageDto
        {
            Id = message.Id,
            CustomerId = message.CustomerId,
            CustomerName = message.Customer != null ? message.Customer.FirstName + " " + message.Customer.LastName : "",
            CustomerEmail = message.Customer?.Email ?? "",
            CompanyId = message.CompanyId,
            Subject = message.Subject,
            Message = message.Message,
            IsRead = message.IsRead,
            IsReplied = message.IsReplied,
            Reply = message.Reply,
            RepliedAt = message.RepliedAt,
            RepliedByUserId = message.RepliedByUserId,
            RepliedByName = message.RepliedBy != null ? message.RepliedBy.FirstName + " " + message.RepliedBy.LastName : null,
            Category = message.Category,
            CategoryText = GetCategoryText(message.Category),
            CreatedAt = message.CreatedAt
        });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CustomerMessageCreateDto dto)
    {
        try
        {
            var message = CustomerMessage.Create(
                dto.CustomerId,
                dto.CompanyId,
                dto.Subject,
                dto.Message,
                dto.Category);

            _context.CustomerMessages.Add(message);
            await _context.SaveChangesAsync();

            return Ok(new { id = message.Id, message = "Mesaj oluşturuldu." });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}/read")]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        var message = await _context.CustomerMessages.FindAsync(id);
        if (message == null)
            return NotFound(new { message = "Mesaj bulunamadı." });

        message.MarkAsRead();
        await _context.SaveChangesAsync();

        return Ok(new { message = "Mesaj okundu olarak işaretlendi." });
    }

    [HttpPost("{id}/reply")]
    public async Task<IActionResult> Reply(int id, [FromBody] CustomerMessageReplyDto dto)
    {
        var message = await _context.CustomerMessages.FindAsync(id);
        if (message == null)
            return NotFound(new { message = "Mesaj bulunamadı." });

        try
        {
            message.SendReply(dto.Reply, dto.RepliedByUserId);
            await _context.SaveChangesAsync();

            // TODO: Müşteriye e-posta bildirimi gönder

            return Ok(new { message = "Yanıt gönderildi." });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var message = await _context.CustomerMessages.FindAsync(id);
        if (message == null)
            return NotFound(new { message = "Mesaj bulunamadı." });

        _context.CustomerMessages.Remove(message);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Mesaj silindi." });
    }

    private static string GetCategoryText(MessageCategory category)
    {
        return category switch
        {
            MessageCategory.General => "Genel",
            MessageCategory.Order => "Sipariş",
            MessageCategory.Product => "Ürün",
            MessageCategory.Return => "İade",
            MessageCategory.Complaint => "Şikayet",
            MessageCategory.Suggestion => "Öneri",
            _ => "Genel"
        };
    }
}
