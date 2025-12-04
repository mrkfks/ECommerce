using ECommerce.Domain.Entities;

namespace ECommerce.Domain.Interfaces
{
    public interface IOrderRepository : IGenericRepository<Order>
    {
        Task<IReadOnlyList<Order>> GetOrdersByCustomerAsync(int customerId, int page, int pageSize);
        Task<decimal> GetCustomerTotalSpentAsync(int customerId);
        Task<bool> UpdateStatusAsync(int orderId, string newStatus);
    }
}