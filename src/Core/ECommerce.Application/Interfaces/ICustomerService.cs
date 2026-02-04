using ECommerce.Application.DTOs;

namespace ECommerce.Application.Interfaces
{
    public interface ICustomerService
    {
        Task<CustomerDto?> GetByIdAsync(int id);
        Task<CustomerDto?> GetByUserIdAsync(int userId);

        Task<IReadOnlyList<CustomerDto>> GetAllAsync();
        Task<ECommerce.Application.Responses.PagedResult<CustomerDto>> GetPagedAsync(int pageNumber, int pageSize);

        Task<IReadOnlyList<CustomerSummaryDto>> GetSummariesAsync();

        Task<CustomerDto> CreateAsync(CustomerFormDto dto);
        Task UpdateAsync(CustomerFormDto dto);

        Task DeleteAsync(int id);

        Task<IReadOnlyList<OrderDto>> GetOrdersAsync(int customerId);

        Task<IReadOnlyList<AddressDto>> GetAddressesAsync(int customerId);

        Task<IReadOnlyList<ReviewDto>> GetReviewsAsync(int customerId);
    }
}
