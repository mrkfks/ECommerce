using ECommerce.Domain.Entities;
using ECommerce.Domain.Interfaces;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Repositories
{
    public class AddressRepository : GenericRepository<Address>, IAddressRepository
    {
        private readonly AppDbContext _context;

        public AddressRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<Address>> GetByCustomerIdAsync(int customerId)
        {
            return await _context.Addresses
                .Where(a => a.CustomerId == customerId)
                .Include(a => a.Customer)
                .OrderByDescending(a => a.Id)
                .ToListAsync();
        }

        public async Task<Address?> GetDefaultAddressAsync(int customerId)
        {
            return await _context.Addresses
                .Where(a => a.CustomerId == customerId)
                .Include(a => a.Customer)
                .FirstOrDefaultAsync();
        }
    }
}
