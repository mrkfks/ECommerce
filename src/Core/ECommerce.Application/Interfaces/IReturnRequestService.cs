using ECommerce.Application.DTOs;
using ECommerce.Domain.Enums;

namespace ECommerce.Application.Interfaces
{
    public interface IReturnRequestService
    {
        Task<ReturnRequestDto?> GetByIdAsync(int id);
        Task<IReadOnlyList<ReturnRequestDto>> GetAllAsync();
        Task<IReadOnlyList<ReturnRequestDto>> GetByOrderIdAsync(int orderId);
        Task<IReadOnlyList<ReturnRequestDto>> GetByCustomerIdAsync(int customerId);
        Task<IReadOnlyList<ReturnRequestDto>> GetMyReturnRequestsAsync(int userId);
        Task<ReturnRequestDto> CreateAsync(CreateReturnRequestDto dto, int userId);
        Task UpdateStatusAsync(int id, UpdateReturnRequestDto dto);
        Task<IReadOnlyList<ReturnRequestDto>> GetPendingRequestsAsync();
    }
}
