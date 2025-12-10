using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;

namespace ECommerce.Domain.Interfaces
{
    public interface IOrderRepository : IGenericRepository<Order>
    {
        Task<IReadOnlyList<Order>> GetOrdersByCustomerAsync(int customerId, int page, int pageSize);
        Task<decimal> GetCustomerTotalSpentAsync(int customerId);
        Task<bool> UpdateStatusAsync(int orderId, OrderStatus newStatus);
        Task<List<Order>> GetByCustomerIdAsync(int customerId);
    }
}