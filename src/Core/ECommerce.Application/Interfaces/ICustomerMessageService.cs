using ECommerce.Application.DTOs;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Interfaces;

namespace ECommerce.Application.Interfaces
{
    public interface ICustomerMessageService : IGenericRepository<CustomerMessage>
    {
        Task<List<CustomerMessage>> GetUnreadMessagesAsync(int companyId);
        Task MarkAsReadAsync(int id);

        // Yeni metodlar - müşteri tarafı
        Task<List<CustomerMessageDto>> GetMyMessagesAsync(int customerId);
        Task<CustomerMessageDto> CreateMessageAsync(CustomerMessageFormDto dto, int customerId);
        Task<int> GetUnreadCountAsync(int customerId);
        Task ReplyToMessageAsync(int messageId, string reply, int userId);
    }
}

