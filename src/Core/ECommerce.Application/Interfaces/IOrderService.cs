using ECommerce.Application.DTOs;

namespace ECommerce.Application.Interfaces
{
    public interface IOrderService
    {
        Task<OrderDto?> GetByIdAsync(int id);
        Task<IReadOnlyList<OrderDto>> GetAllAsync();
        Task<IReadOnlyList<OrderDto>> GetByCustomerIdAsync(int customerId);
        Task<OrderDto> CreateAsync(OrderCreateDto dto);
        Task UpdateAsync(OrderUpdateDto dto);
        Task DeleteAsync(int id);
        Task UpdateStatusAsync(int orderId, string status);
        Task AddItemAsync(int orderId, OrderItemCreateDto itemDto);
        Task RemoveItemAsync(int orderId, int productId);
    }
}

