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

        public CustomerMessageService(AppDbContext context, ILogger<CustomerMessageService> logger) : base(context)
        {
            _context = context;
            _logger = logger;
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
    }
}
