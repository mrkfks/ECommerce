using ECommerce.Application.DTOs;

namespace ECommerce.Application.Interfaces
{
    public interface ICustomerService
    {
        Task<CustomerDto?> GetByIdAsync(int id);

        Task<IReadOnlyList<CustomerDto>> GetAllAsync();

        Task<IReadOnlyList<CustomerSummaryDto>> GetSummariesAsync();

        Task<CustomerDto> CreateAsync(CustomerCreateDto dto);

        Task UpdateAsync(CustomerUpdateDto dto);

        Task DeleteAsync(int id);

        Task<IReadOnlyList<OrderDto>> GetOrdersAsync(int customerId);

        Task<IReadOnlyList<AddressDto>> GetAddressesAsync(int customerId);

        Task<IReadOnlyList<ReviewDto>> GetReviewsAsync(int customerId);
    }
}
