using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Interfaces;
using ECommerce.Infrastructure.Data;
using ECommerce.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Repositories
{
    public class ProductRepository : GenericRepository<Product>, IProductRepository
    {
        private readonly AppDbContext _context;
        private readonly ITenantService _tenantService;

        public ProductRepository(AppDbContext context, ITenantService tenantService) : base(context)
        {
            _context = context;
            _tenantService = tenantService;
        }

        public async Task<IReadOnlyList<Product>> GetPageAsync(int page, int pageSize, int? categoryId, int? BrandId, string? search)
        {
            var query = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.Company)
                .AsQueryable();

            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            if (BrandId.HasValue)
            {
                query = query.Where(p => p.BrandId == BrandId.Value);
            }

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p => p.Name.Contains(search) || p.Description.Contains(search));
            }

            return await query
                .Where(p => p.IsActive)
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<bool> IsStockAvailableAsync(int productId, int quantity)
        {
            var product = await _context.Products.FindAsync(productId);
            return product != null && product.IsActive && product.StockQuantity >= quantity;
        }

        public async Task<bool> IsProductNameUniqueAsync(string name, int? excludeId = null)
        {
            var query = _context.Products.Where(p => p.Name == name);

            if (excludeId.HasValue)
            {
                query = query.Where(p => p.Id != excludeId.Value);
            }

            return !await query.AnyAsync();
        }

        public async Task<List<Product>> GetByCategoryAsync(int categoryId)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.Company)
                .Where(p => p.CategoryId == categoryId && p.IsActive)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Product>> SearchAsync(string searchTerm)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.Company)
                .Where(p => (p.Name.Contains(searchTerm) || p.Description.Contains(searchTerm)) && p.IsActive)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<IReadOnlyList<Product>> GetAllWithDetailsAsync()
        {
            // Query filter'ı bypass edip manuel filtreleme
            var currentCompanyId = _tenantService.GetCompanyId();

            var query = _context.Products
               .IgnoreQueryFilters()  // Global filter'ı kapat
               .Include(p => p.Category)
               .Include(p => p.Brand)
               .Include(p => p.Company)
               .Where(p => !p.IsDeleted && p.IsActive);

            // Eğer company context varsa, ona göre filtrele
            if (currentCompanyId.HasValue)
            {
                query = query.Where(p => p.CompanyId == currentCompanyId.Value);
            }

            return await query
               .OrderByDescending(p => p.CreatedAt)
               .ToListAsync();
        }

        public new async Task<IReadOnlyList<Product>> GetPagedAsync(int pageNumber, int pageSize)
        {
            return await GetPageAsync(pageNumber, pageSize, null, null, null);
        }
    }
}