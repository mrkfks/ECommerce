using ECommerce.Domain.Entities;
using ECommerce.Domain.Interfaces;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

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
                .Include(c => c.Addresses)
                .Include(c => c.User)
                .Include(c => c.Company)
                .FirstOrDefaultAsync(c => c.Email == email);
        }

        public async Task<IReadOnlyList<Customer>> GetWithOrdersAsync()
        {
            return await _context.Customers
                .Include(c => c.Orders)
                .Include(c => c.User)
                .Include(c => c.Company)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }
    }
}