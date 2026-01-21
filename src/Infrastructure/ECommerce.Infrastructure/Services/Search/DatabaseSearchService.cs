using ECommerce.Application.DTOs;
using ECommerce.Application.DTOs.Common;
using ECommerce.Application.Interfaces;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Services.Search
{
    public class DatabaseSearchService : ISearchService
    {
        private readonly AppDbContext _context;

        public DatabaseSearchService(AppDbContext context)
        {
            _context = context;
        }

        public Task<bool> IndexProductAsync(ProductDto product) => Task.FromResult(true); // No-op for DB
        public Task<bool> DeleteProductAsync(int productId) => Task.FromResult(true); // No-op

        public async Task<PaginatedResult<ProductDto>> SearchProductsAsync(string query, int page, int pageSize, int? categoryId = null)
        {
            var dbQuery = _context.Products.AsNoTracking().AsQueryable();

            if (categoryId.HasValue)
                dbQuery = dbQuery.Where(p => p.CategoryId == categoryId.Value);

            if (!string.IsNullOrWhiteSpace(query))
            {
                var lowerQuery = query.ToLower();
                // Simple SQL LIKE search
                dbQuery = dbQuery.Where(p => p.Name.ToLower().Contains(lowerQuery) || 
                                             p.Description.ToLower().Contains(lowerQuery));
            }

            var totalCount = await dbQuery.CountAsync();
            var items = await dbQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    CategoryId = p.CategoryId,
                    // Map other fields... simplified for search result
                })
                .ToListAsync();

            return new PaginatedResult<ProductDto>(items, totalCount, page, pageSize);
        }
    }
}
