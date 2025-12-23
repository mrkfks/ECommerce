using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using ECommerce.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using ECommerce.Infrastructure.Data;

namespace ECommerce.Infrastructure.Repositories
{
    public class OrderRepository : GenericRepository<Order>, IOrderRepository
    {
        private readonly AppDbContext _context;
        public OrderRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }
        
        public async Task<IReadOnlyList<Order>> GetOrdersByCustomerAsync(int customerId, int page, int pageSize)
        {
            return await _context.Orders
                .Where(o => o.CustomerId == customerId)
                .Include(o => o.Items)
                    .ThenInclude(oi => oi.Product)
                .Include(o => o.Address)
                .Include(o => o.Customer)
                .OrderByDescending(o => o.OrderDate)
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
            
            // Rich Domain Model: Entity üzerindeki metotları kullan
            switch(newStatus)
            {
                case OrderStatus.Processing:
                    order.Confirm();
                    break;
                case OrderStatus.Shipped:
                    order.Ship();
                    break;
                case OrderStatus.Delivered:
                    order.Deliver();
                    break;
                case OrderStatus.Cancelled:
                    order.Cancel();
                    break;
            }
            
            _context.Orders.Update(order);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<Order>> GetByCustomerIdAsync(int customerId)
        {
            return await _context.Orders
                .Where(o => o.CustomerId == customerId)
                .Include(o => o.Items)
                    .ThenInclude(oi => oi.Product)
                .Include(o => o.Address)
                .Include(o => o.Customer)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }
    }
}