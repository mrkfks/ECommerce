using ECommerce.Domain.Entities;
using ECommerce.Domain.Interfaces;

namespace ECommerce.Infrastructure.Repositories
{
    public class CustomerRepository : GenericRepository<Customer>, ICustomerRepository
    {
        private readonly AppDbContext _context;
        public CustomerRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }
        public async Task<Customer?> GetByEmailAsync(string email)
        {
            return await _context.Customers
            .Include(c => c.Orders)
            .FirstOrDefaultAsync(c => c.Email == email);
        }
        public async Task<IReadOnlyList<Customer>> GetCustomersWithOrdersAsync()
        {
            return await _context.Customers
            .Include(c => c.Orders)
            .ToListAsync();
        }
    }
}