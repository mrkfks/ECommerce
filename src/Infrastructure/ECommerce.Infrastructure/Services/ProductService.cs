using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Services
{
    public class ProductService : IProductService
    {
        private readonly AppDbContext _context;

        public ProductService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ProductDto?> GetByIdAsync(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .FirstOrDefaultAsync(p => p.Id == id);
                
            if (product == null) return null;
            return MapToDto(product);
        }

        public async Task<IReadOnlyList<ProductDto>> GetAllAsync()
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Select(p => MapToDto(p))
                .ToListAsync();
        }

        public async Task<IReadOnlyList<ProductDto>> GetByCategoryIdAsync(int categoryId)
        {
            return await _context.Products
                .Where(p => p.CategoryId == categoryId)
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Select(p => MapToDto(p))
                .ToListAsync();
        }

        public async Task<IReadOnlyList<ProductDto>> GetByBrandIdAsync(int brandId)
        {
            return await _context.Products
                .Where(p => p.BrandId == brandId)
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Select(p => MapToDto(p))
                .ToListAsync();
        }

        public async Task<IReadOnlyList<ProductDto>> GetByCompanyAsync(int companyId)
        {
            return await _context.Products
                .Where(p => p.CompanyId == companyId)
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Select(p => MapToDto(p))
                .ToListAsync();
        }

        public async Task<ProductDto> CreateAsync(ProductCreateDto dto)
        {
            var product = new Product
            {
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                StockQuantity = dto.StockQuantity,
                CategoryId = dto.CategoryId,
                BrandId = dto.BrandId,
                CompanyId = dto.CompanyId,
                ImageUrl = dto.ImageUrl,
                IsActive = true
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return MapToDto(product);
        }

        public async Task UpdateAsync(ProductUpdateDto dto)
        {
            var product = await _context.Products.FindAsync(dto.Id);
            if (product == null) return;

            product.Name = dto.Name;
            product.Description = dto.Description;
            product.Price = dto.Price;
            product.StockQuantity = dto.StockQuantity;
            product.CategoryId = dto.CategoryId;
            product.BrandId = dto.BrandId;
            product.ImageUrl = dto.ImageUrl;
            product.IsActive = dto.IsActive;
            
            await _context.SaveChangesAsync();
        }
        
        // This overload was used in the Controller: UpdateAsync(id, name, price, stock...)
        // But Interface has UpdateAsync(ProductUpdateDto). 
        // The Controller calls: _productService.UpdateAsync(id, dto.Name, dto.Price, dto.Stock, dto.CompanyId, dto.CategoryId);
        // This means the Controller expects a method with multiple params.
        // However, IProductService has: Task UpdateAsync(ProductUpdateDto dto);
        // I should probably conform to the Interface, and fix the Controller to use the DTO method.
        // But for now, to satisfy the Controller compilation, I might need to overload or fix the Controller.
        // The prompt asked me to fix the build. The Controller uses:
        // UpdateAsync(int, string, decimal, int, int, int)
        // I will implement the Interface method primarily. I will FIX the Controller to call this method properly.

        public async Task DeleteAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }
        }

        public async Task UpdateStockAsync(int productId, int newQuantity)
        {
             var product = await _context.Products.FindAsync(productId);
             if(product != null)
             {
                 product.StockQuantity = newQuantity;
                 await _context.SaveChangesAsync();
             }
        }

        public async Task<IReadOnlyList<ProductDto>> SearchAsync(string keyword)
        {
             return await _context.Products
                .Where(p => p.Name.Contains(keyword) || p.Description.Contains(keyword))
                .Select(p => MapToDto(p))
                .ToListAsync();
        }

        private static ProductDto MapToDto(Product p)
        {
            return new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                StockQuantity = p.StockQuantity,
                CompanyId = p.CompanyId,
                CategoryId = p.CategoryId,
                CategoryName = p.Category?.Name ?? "",
                BrandId = p.BrandId,
                BrandName = p.Brand?.Name ?? "",
                CompanyName = "",
                ImageUrl = p.ImageUrl,
                IsActive = p.IsActive,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt,
                ReviewCount = 0,
                AverageRating = 0
            };
        }
    }
}
