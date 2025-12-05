using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;

namespace ECommerce.Infrastructure.Repositories
{
    public class OrderRepository : GenericRepository<Order>, IOrderRepository
    {
        private readonly AppDbContext _context;
        public OrderRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }
        public async Task<IReadOnlyList<Order>> GetOrdersByCustomerIdAsync(int customerId, int page, int pageSize)
        {
            return await _context.Orders
            .Where(o => o.CustomerId == customerId)
            .Include(o => o.OrderItems)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        }
        public async Task<decimal> GetCustomerTotalSpentAsync(int customerId)
        {
            return await _context.Orders
            .Where(o => o.CustomerId == customerId)
            .SumAsync(o => o.TotalAmount);
        }
        public async Task<bool> UpdateStatusAsync(int orderId, OrderStatus newStatus)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
            {
                return false;
            }
            order.Status = newStatus;
            _context.Orders.Update(order);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}