using ECommerce.Domain.Entities;
using ECommerce.Domain.Interfaces;

namespace ECommerce.Application.Interfaces
{
    public interface ICustomerMessageService : IGenericRepository<CustomerMessage>
    {
        Task<List<CustomerMessage>> GetUnreadMessagesAsync(int companyId);
        Task MarkAsReadAsync(int id);
    }
}
