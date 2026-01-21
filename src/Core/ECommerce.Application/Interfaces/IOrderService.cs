using ECommerce.Application.DTOs;
using ECommerce.Domain.Enums;

namespace ECommerce.Application.Interfaces
{
    public interface IOrderService
    {
        Task<OrderDto?> GetByIdAsync(int id);
        Task<IReadOnlyList<OrderDto>> GetAllAsync();
        Task<IReadOnlyList<OrderDto>> GetByCustomerIdAsync(int customerId);
        Task<IReadOnlyList<OrderDto>> GetMyOrdersAsync(int userId);
        Task<OrderDto> CreateAsync(OrderCreateDto dto);
        Task UpdateAsync(OrderUpdateDto dto);
        Task DeleteAsync(int id);
        Task UpdateStatusAsync(int orderId, OrderStatus status);
        Task AddItemAsync(int orderId, OrderItemCreateDto itemDto);
        Task RemoveItemAsync(int orderId, int productId);
    }
}

