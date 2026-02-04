using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Data;
using ECommerce.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ECommerce.Infrastructure.Services
{
    public class CustomerMessageService : GenericRepository<CustomerMessage>, ICustomerMessageService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<CustomerMessageService> _logger;
        private readonly ITenantService _tenantService;

        public CustomerMessageService(AppDbContext context, ILogger<CustomerMessageService> logger, ITenantService tenantService) : base(context)
        {
            _context = context;
            _logger = logger;
            _tenantService = tenantService;
        }

        public async Task<List<CustomerMessage>> GetUnreadMessagesAsync(int companyId)
        {
            return await _context.CustomerMessages
                .Where(m => m.CompanyId == companyId && !m.IsRead)
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();
        }

        public async Task MarkAsReadAsync(int id)
        {
            var message = await _context.CustomerMessages.FindAsync(id);
            if (message == null) throw new KeyNotFoundException("Mesaj bulunamadı.");

            message.MarkAsRead();
            await _context.SaveChangesAsync();
        }

        public async Task<List<CustomerMessageDto>> GetMyMessagesAsync(int customerId)
        {
            var messages = await _context.CustomerMessages
                .Include(m => m.Customer)
                .Include(m => m.RepliedBy)
                .Where(m => m.CustomerId == customerId)
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();

            return messages.Select(MapToDto).ToList();
        }

        public async Task<CustomerMessageDto> CreateMessageAsync(CustomerMessageFormDto dto, int customerId)
        {
            var companyId = _tenantService.GetCompanyId() ?? throw new Exception("Şirket bilgisi bulunamadı.");

            var message = CustomerMessage.Create(
                customerId,
                companyId,
                dto.Subject,
                dto.Message,
                dto.Category
            );

            _context.CustomerMessages.Add(message);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Customer message created - Id: {Id}, CustomerId: {CustomerId}", message.Id, customerId);

            return MapToDto(message);
        }

        public async Task<int> GetUnreadCountAsync(int customerId)
        {
            return await _context.CustomerMessages
                .Where(m => m.CustomerId == customerId && m.IsReplied && !m.IsRead)
                .CountAsync();
        }

        public async Task ReplyToMessageAsync(int messageId, string reply, int userId)
        {
            var message = await _context.CustomerMessages.FindAsync(messageId);
            if (message == null) throw new KeyNotFoundException("Mesaj bulunamadı.");

            message.SendReply(reply, userId);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Message replied - Id: {Id}, UserId: {UserId}", messageId, userId);
        }

        private static CustomerMessageDto MapToDto(CustomerMessage m)
        {
            return new CustomerMessageDto(
                m.Id,
                m.CustomerId,
                m.Customer != null ? $"{m.Customer.FirstName} {m.Customer.LastName}" : "",
                m.Customer?.Email ?? "",
                m.CompanyId,
                m.Subject,
                m.Message,
                m.IsRead,
                m.IsReplied,
                m.Reply,
                m.RepliedAt,
                m.RepliedByUserId,
                m.RepliedBy != null ? $"{m.RepliedBy.FirstName} {m.RepliedBy.LastName}" : null,
                m.Category,
                m.Category.ToString(),
                m.CreatedAt
            );
        }
    }
}

