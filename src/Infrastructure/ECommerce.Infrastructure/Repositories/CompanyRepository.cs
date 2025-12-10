using ECommerce.Domain.Entities;
using ECommerce.Domain.Interfaces;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Repositories
{
    public class CompanyRepository : GenericRepository<Company>, ICompanyRepository
    {
        private readonly AppDbContext _context;

        public CompanyRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Company?> GetByTaxNumberAsync(string taxNumber)
        {
            return await _context.Companies
                .Include(c => c.Users)
                .Include(c => c.Customers)
                .FirstOrDefaultAsync(c => c.TaxNumber == taxNumber);
        }

        public async Task<Company?> GetByEmailAsync(string email)
        {
            return await _context.Companies
                .Include(c => c.Users)
                .FirstOrDefaultAsync(c => c.Email == email);
        }

        public async Task<IReadOnlyList<Company>> GetActiveCompaniesAsync()
        {
            return await _context.Companies
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }
    }
}
