using ECommerce.Application.DTOs;
using ECommerce.Application.Responses;

namespace ECommerce.Application.Interfaces
{
    public interface ICustomerMessageService
    {
        Task<IReadOnlyList<CustomerMessageDto>> GetAllAsync();
        Task<PagedResult<CustomerMessageDto>> GetPagedAsync(int pageNumber, int pageSize);
        Task<IReadOnlyList<CustomerMessageDto>> GetUnreadAsync();
        Task<IReadOnlyList<CustomerMessageDto>> GetPendingAsync();
        Task<CustomerMessageSummaryDto> GetSummaryAsync();
        Task<CustomerMessageDto?> GetByIdAsync(int id);
        Task<int> CreateAsync(CustomerMessageCreateDto dto);
        Task MarkAsReadAsync(int id);
        Task SendReplyAsync(int id, CustomerMessageReplyDto dto);
        Task DeleteAsync(int id);
    }
}
