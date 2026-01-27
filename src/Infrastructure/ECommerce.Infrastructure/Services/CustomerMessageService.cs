using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Responses;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ECommerce.Infrastructure.Services
{
    public class CustomerMessageService : ICustomerMessageService
    {
        private readonly AppDbContext _context;
        private readonly ITenantService _tenantService;
        private readonly ILogger<CustomerMessageService> _logger;

        public CustomerMessageService(AppDbContext context, ITenantService tenantService, ILogger<CustomerMessageService> logger)
        {
            _context = context;
            _tenantService = tenantService;
            _logger = logger;
        }

        public async Task<IReadOnlyList<CustomerMessageDto>> GetAllAsync()
        {
            var companyId = _tenantService.GetCompanyId();
            var query = _context.CustomerMessages.AsNoTracking();

            if (companyId.HasValue)
                query = query.Where(m => m.CompanyId == companyId.Value);

            var messages = await query
                .Include(m => m.Customer)
                .Include(m => m.RepliedBy)
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();

            return messages.Select(MapToDto).ToList();
        }

        public async Task<PagedResult<CustomerMessageDto>> GetPagedAsync(int pageNumber, int pageSize)
        {
            var companyId = _tenantService.GetCompanyId();
            var query = _context.CustomerMessages.AsNoTracking();

            if (companyId.HasValue)
                query = query.Where(m => m.CompanyId == companyId.Value);

            var totalCount = await query.CountAsync();
            var messages = await query
                .Include(m => m.Customer)
                .Include(m => m.RepliedBy)
                .OrderByDescending(m => m.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<CustomerMessageDto>
            {
                Items = messages.Select(MapToDto).ToList(),
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<IReadOnlyList<CustomerMessageDto>> GetUnreadAsync()
        {
            var companyId = _tenantService.GetCompanyId();
            var query = _context.CustomerMessages.AsNoTracking().Where(m => !m.IsRead);

            if (companyId.HasValue)
                query = query.Where(m => m.CompanyId == companyId.Value);

            var messages = await query
                .Include(m => m.Customer)
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();

            return messages.Select(MapToDto).ToList();
        }

        public async Task<IReadOnlyList<CustomerMessageDto>> GetPendingAsync()
        {
            var companyId = _tenantService.GetCompanyId();
            var query = _context.CustomerMessages.AsNoTracking().Where(m => !m.IsReplied);

            if (companyId.HasValue)
                query = query.Where(m => m.CompanyId == companyId.Value);

            var messages = await query
                .Include(m => m.Customer)
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();

            return messages.Select(MapToDto).ToList();
        }

        public async Task<CustomerMessageSummaryDto> GetSummaryAsync()
        {
            var companyId = _tenantService.GetCompanyId();
            var query = _context.CustomerMessages.AsNoTracking();

            if (companyId.HasValue)
                query = query.Where(m => m.CompanyId == companyId.Value);

            var messages = await query.ToListAsync();

            return new CustomerMessageSummaryDto
            {
                TotalMessages = messages.Count,
                UnreadMessages = messages.Count(m => !m.IsRead),
                PendingReplies = messages.Count(m => !m.IsReplied),
                RepliedMessages = messages.Count(m => m.IsReplied),
                RecentMessages = messages
                    .OrderByDescending(m => m.CreatedAt)
                    .Take(10)
                    .Select(MapToDto)
                    .ToList()
            };
        }

        public async Task<CustomerMessageDto?> GetByIdAsync(int id)
        {
            var message = await _context.CustomerMessages
                .Include(m => m.Customer)
                .Include(m => m.RepliedBy)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);

            return message == null ? null : MapToDto(message);
        }

        public async Task<int> CreateAsync(CustomerMessageCreateDto dto)
        {
            var message = CustomerMessage.Create(
                dto.CustomerId,
                dto.CompanyId,
                dto.Subject,
                dto.Message,
                dto.Category);

            _context.CustomerMessages.Add(message);
            await _context.SaveChangesAsync();

            return message.Id;
        }

        public async Task MarkAsReadAsync(int id)
        {
            var message = await _context.CustomerMessages.FindAsync(id);
            if (message == null) throw new KeyNotFoundException("Mesaj bulunamadı.");

            message.MarkAsRead();
            await _context.SaveChangesAsync();
        }

        public async Task SendReplyAsync(int id, CustomerMessageReplyDto dto)
        {
            var message = await _context.CustomerMessages.FindAsync(id);
            if (message == null) throw new KeyNotFoundException("Mesaj bulunamadı.");

            message.SendReply(dto.Reply, dto.RepliedByUserId);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Reply sent for message {MessageId} by user {UserId}", id, dto.RepliedByUserId);
        }

        public async Task DeleteAsync(int id)
        {
            var message = await _context.CustomerMessages.FindAsync(id);
            if (message == null) throw new KeyNotFoundException("Mesaj bulunamadı.");

            _context.CustomerMessages.Remove(message);
            await _context.SaveChangesAsync();
        }

        private static CustomerMessageDto MapToDto(CustomerMessage m)
        {
            return new CustomerMessageDto
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
            };
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
}
