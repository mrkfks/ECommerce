using AutoMapper;
using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ECommerce.Infrastructure.Services
{
    public class ProductService : IProductService
    {
        private readonly AppDbContext _context;
        private readonly ITenantService _tenantService;
        private readonly IMapper _mapper;
        private readonly ILogger<ProductService> _logger;
        private readonly IStorageService _storageService;
        private readonly ISearchService _searchService;

        public ProductService(AppDbContext context, ITenantService tenantService, IMapper mapper, ILogger<ProductService> logger, IStorageService storageService, ISearchService searchService)
        {
            _context = context;
            _tenantService = tenantService;
            _mapper = mapper;
            _logger = logger;
            _storageService = storageService;
            _searchService = searchService;
        }

        public async Task<ProductDto> CreateAsync(ProductFormDto dto)
        {
            var companyId = _tenantService.GetCompanyId();
            int effectiveCompanyId = dto.CompanyId;
            if (companyId.HasValue && companyId.Value != dto.CompanyId)
            {
                effectiveCompanyId = companyId.Value;
            }

            var product = Product.Create(
                dto.Name,
                dto.Description ?? string.Empty,
                dto.Price,
                dto.CategoryId,
                dto.BrandId,
                effectiveCompanyId,
                dto.StockQuantity,
                dto.ModelId
            );

            if (!string.IsNullOrEmpty(dto.ImageUrl))
            {
                // Add primary image if URL provided
                var img = ProductImage.Create(product.Id, dto.ImageUrl, 0, true);
                product.Images.Add(img);
            }

            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            var productDto = MapToDto(product);
            // Elasticsearch'e ekle
            try { await _searchService.IndexProductAsync(productDto); } catch (Exception ex) { _logger.LogError(ex, "Elasticsearch index ekleme hatası"); }
            return productDto;
        }

        public async Task DeleteAsync(int id)
        {
            var product = await _context.Products
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null) throw new Exception("Product not found");

            // Delete physical images
            if (product.Images != null && product.Images.Any())
            {
                foreach (var img in product.Images)
                {
                    if (!string.IsNullOrEmpty(img.ImageUrl))
                        await _storageService.DeleteFileAsync(img.ImageUrl);
                }
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            // Elasticsearch'ten sil
            try { await _searchService.DeleteProductAsync(id); } catch (Exception ex) { _logger.LogError(ex, "Elasticsearch index silme hatası"); }
        }

        public async Task<IReadOnlyList<ProductDto>> GetAllAsync()
        {
            var companyId = _tenantService.GetCompanyId();
            var query = _context.Products.AsNoTracking();

            if (companyId.HasValue)
                query = query.Where(p => p.CompanyId == companyId.Value);

            var products = await query
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.Model)
                .Include(p => p.Images)
                .OrderBy(p => p.Name)
                .ToListAsync();

            return products.Select(MapToDto).ToList();
        }

        public async Task<ECommerce.Application.Responses.PagedResult<ProductDto>> GetPagedAsync(int pageNumber, int pageSize)
        {
            var companyId = _tenantService.GetCompanyId();
            var query = _context.Products.AsNoTracking();

            if (companyId.HasValue)
                query = query.Where(p => p.CompanyId == companyId.Value);

            var totalCount = await query.CountAsync();

            var products = await query
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.Model)
                .Include(p => p.Images)
                .OrderBy(p => p.Name)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var dtos = products.Select(MapToDto).ToList();

            return new ECommerce.Application.Responses.PagedResult<ProductDto>
            {
                Items = dtos,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<IReadOnlyList<ProductDto>> GetByBrandIdAsync(int brandId)
        {
            var products = await _context.Products
               .Include(p => p.Category)
               .Include(p => p.Brand)
               .Include(p => p.Model)
               .Include(p => p.Images)
               .Where(p => p.BrandId == brandId)
               .AsNoTracking()
               .ToListAsync();
            return products.Select(MapToDto).ToList();
        }

        public async Task<IReadOnlyList<ProductDto>> GetByCategoryIdAsync(int categoryId)
        {
            var products = await _context.Products
               .Include(p => p.Category)
               .Include(p => p.Brand)
               .Include(p => p.Model)
               .Include(p => p.Images)
               .Where(p => p.CategoryId == categoryId)
               .AsNoTracking()
               .ToListAsync();
            return products.Select(MapToDto).ToList();
        }

        public async Task<IReadOnlyList<ProductDto>> GetByCompanyAsync(int companyId)
        {
            var products = await _context.Products
               .Include(p => p.Category)
               .Include(p => p.Brand)
               .Include(p => p.Model)
               .Include(p => p.Images)
               .Where(p => p.CompanyId == companyId)
               .AsNoTracking()
               .ToListAsync();
            return products.Select(MapToDto).ToList();
        }

        public async Task<ProductDto?> GetByIdAsync(int id)
        {
            var product = await _context.Products
               .Include(p => p.Category)
               .Include(p => p.Brand)
               .Include(p => p.Model)
               .Include(p => p.Images)
               .FirstOrDefaultAsync(p => p.Id == id);

            return product == null ? null : MapToDto(product);
        }

        public async Task<IReadOnlyList<ProductDto>> SearchAsync(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword)) return await GetAllAsync();

            var companyId = _tenantService.GetCompanyId();
            var query = _context.Products.AsNoTracking();
            if (companyId.HasValue) query = query.Where(p => p.CompanyId == companyId.Value);

            var products = await query
               .Include(p => p.Category)
               .Include(p => p.Brand)
               .Include(p => p.Model)
               .Include(p => p.Images)
               .Where(p => p.Name.Contains(keyword) || (p.Description != null && p.Description.Contains(keyword)))
               .ToListAsync();

            return products.Select(MapToDto).ToList();
        }

        public async Task UpdateAsync(ProductFormDto dto)
        {
            if (!dto.Id.HasValue) throw new Exception("Product Id is required for update");
            var product = await _context.Products.FindAsync(dto.Id.Value);
            if (product == null) throw new Exception("Product not found");

            product.Update(
                dto.Name,
                dto.Description ?? string.Empty,
                dto.Price
            );

            product.SetModel(dto.ModelId);
            product.UpdateStock(dto.StockQuantity);

            if (dto.IsActive && !product.IsActive) product.Activate();
            else if (!dto.IsActive && product.IsActive) product.Deactivate();

            await _context.SaveChangesAsync();
            // Elasticsearch'te güncelle (indexle)
            var updatedProduct = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.Model)
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == dto.Id.Value);
            if (updatedProduct != null)
            {
                var productDto = MapToDto(updatedProduct);
                try { await _searchService.IndexProductAsync(productDto); } catch (Exception ex) { _logger.LogError(ex, "Elasticsearch index güncelleme hatası"); }
            }
        }

        public async Task UpdateStockAsync(int productId, int newQuantity)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null) throw new Exception("Product not found");

            product.UpdateStock(newQuantity);
            await _context.SaveChangesAsync();
        }

        public async Task DecreaseStockAsync(int productId, int quantity)
        {
            // Atomik Stok Düşme (İstenen Yapı)
            var affectedRows = await _context.Database.ExecuteSqlInterpolatedAsync(
                $"UPDATE Products SET StockQuantity = StockQuantity - {quantity} WHERE Id = {productId} AND StockQuantity >= {quantity}");

            if (affectedRows == 0)
                throw new Exception($"Stok düşülemedi. Yetersiz stok veya ürün bulunamadı. ProductId: {productId}");
        }

        // Image Methods
        public async Task<ProductImageDto> AddImageAsync(int productId, string imageUrl, int order, bool isPrimary)
        {
            var product = await _context.Products.Include(p => p.Images).FirstOrDefaultAsync(p => p.Id == productId);
            if (product == null) throw new KeyNotFoundException("Product not found");

            // Check permissions based on tenant logic if needed, but Controller handles authorization mostly.
            // Service should ideally check tenant ownership.
            var companyId = _tenantService.GetCompanyId();
            if (companyId.HasValue && product.CompanyId != companyId.Value)
                throw new UnauthorizedAccessException("Not authorized for this product");

            if (isPrimary)
            {
                foreach (var img in product.Images.Where(i => i.IsPrimary)) img.UnsetPrimary();
            }

            var newImg = ProductImage.Create(productId, imageUrl, order, isPrimary);
            product.Images.Add(newImg);
            await _context.SaveChangesAsync();

            return new ProductImageDto
            {
                Id = newImg.Id,
                ProductId = newImg.ProductId,
                ImageUrl = newImg.ImageUrl,
                Order = newImg.Order,
                IsPrimary = newImg.IsPrimary
            };
        }

        public async Task UpdateImageAsync(int productId, int imageId, string imageUrl, int order, bool isPrimary)
        {
            var product = await _context.Products.Include(p => p.Images).FirstOrDefaultAsync(p => p.Id == productId);
            if (product == null) throw new KeyNotFoundException("Product not found");

            var companyId = _tenantService.GetCompanyId();
            if (companyId.HasValue && product.CompanyId != companyId.Value)
                throw new UnauthorizedAccessException("Not authorized for this product");

            var image = product.Images.FirstOrDefault(i => i.Id == imageId);
            if (image == null) throw new KeyNotFoundException("Image not found");

            if (isPrimary && !image.IsPrimary)
            {
                foreach (var img in product.Images.Where(i => i.IsPrimary && i.Id != imageId)) img.UnsetPrimary();
            }

            image.Update(imageUrl, order, isPrimary);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveImageAsync(int productId, int imageId)
        {
            var product = await _context.Products.Include(p => p.Images).FirstOrDefaultAsync(p => p.Id == productId);
            if (product == null) throw new KeyNotFoundException("Product not found");

            var companyId = _tenantService.GetCompanyId();
            if (companyId.HasValue && product.CompanyId != companyId.Value)
                throw new UnauthorizedAccessException("Not authorized for this product");

            var image = product.Images.FirstOrDefault(i => i.Id == imageId);
            if (image == null) throw new KeyNotFoundException("Image not found");

            // Delete physical file
            if (!string.IsNullOrEmpty(image.ImageUrl))
            {
                await _storageService.DeleteFileAsync(image.ImageUrl);
            }

            product.Images.Remove(image);
            // Also explicitly remove from context to be safe if cascade isn't enough or for soft delete logic
            _context.ProductImages.Remove(image);

            await _context.SaveChangesAsync();
        }

        public async Task<List<ProductImageDto>> GetImagesAsync(int productId)
        {
            var product = await _context.Products.AsNoTracking().Include(p => p.Images).FirstOrDefaultAsync(p => p.Id == productId);
            if (product == null) throw new KeyNotFoundException("Product not found");

            // Read access checks? usually public images.

            return product.Images.OrderBy(i => i.Order).Select(i => new ProductImageDto
            {
                Id = i.Id,
                ProductId = i.ProductId,
                ImageUrl = i.ImageUrl,
                Order = i.Order,
                IsPrimary = i.IsPrimary
            }).ToList();
        }

        public async Task BulkUpdatePriceAsync(List<int> productIds, decimal percentage)
        {
            if (productIds == null || !productIds.Any()) return;

            var companyId = _tenantService.GetCompanyId();

            // Base query filtered by tenant
            var query = _context.Products.Where(p => productIds.Contains(p.Id));
            if (companyId.HasValue)
                query = query.Where(p => p.CompanyId == companyId.Value);

            // Calculate multiplier: e.g. 10% -> 1.10
            // ExecuteUpdateAsync with expression tree might be tricky with decimal math in all providers, but usually works in modern EF Core.
            // Price = Price * (1 + percentage/100)

            // Note: EF Core 7+ ExecuteUpdate supports math operations.
            // We need to be careful about precision. 
            // Let's assume Price is decimal(18,2).

            decimal multiplier = 1 + (percentage / 100m);

            await query.ExecuteUpdateAsync(s => s.SetProperty(p => p.Price, p => p.Price * multiplier));
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
                CategoryId = p.CategoryId,
                CategoryName = p.Category?.Name ?? string.Empty,
                BrandId = p.BrandId,
                BrandName = p.Brand?.Name ?? string.Empty,
                ModelId = p.ModelId,
                ModelName = p.Model?.Name,
                CompanyId = p.CompanyId,
                IsActive = p.IsActive,
                ImageUrl = p.Images.FirstOrDefault(i => i.IsPrimary)?.ImageUrl ?? p.Images.FirstOrDefault()?.ImageUrl
            };
        }
    }
}
