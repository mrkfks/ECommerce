using ECommerce.Application.DTOs;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.RestApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "SameCompanyOrSuperAdmin")]
public class ProductController : ControllerBase
{
    private readonly AppDbContext _context;

    public ProductController(AppDbContext context)
    {
        _context = context;
    }
    
    [HttpPost]
    [Authorize(Roles = "CompanyAdmin,User,SuperAdmin")]
    public async Task<IActionResult> Create(ProductCreateDto dto)
    {
        var product = Product.Create(
            dto.Name,
            dto.Description,
            dto.Price,
            dto.CategoryId,
            dto.BrandId,
            dto.CompanyId,
            dto.StockQuantity,
            null, // modelId - opsiyonel
            dto.ImageUrl,
            null  // sku - opsiyonel
        );
        
        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        
        return Ok(new { id = product.Id, message = "Ürün oluşturuldu" });
    }
    
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> Get(int id)
    {
        var product = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .Include(p => p.Company)
            .Include(p => p.Reviews)
            .AsNoTracking()
            .Where(p => p.Id == id)
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                StockQuantity = p.StockQuantity,
                CategoryId = p.CategoryId,
                CategoryName = p.Category != null ? p.Category.Name : "",
                BrandId = p.BrandId,
                BrandName = p.Brand != null ? p.Brand.Name : "",
                CompanyId = p.CompanyId,
                CompanyName = p.Company != null ? p.Company.Name : "",
                ImageUrl = p.ImageUrl,
                IsActive = p.IsActive,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt,
                ReviewCount = p.Reviews.Count,
                AverageRating = p.Reviews.Any() ? p.Reviews.Average(r => r.Rating) : 0
            })
            .FirstOrDefaultAsync();
            
        if (product == null)
            return NotFound(new { message = "Product not found" });
        return Ok(product);
    }
    
    [HttpGet("company/{companyId}")]
    [Authorize]
    public async Task<IActionResult> GetByCompany(int companyId)
    {
        var products = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .Include(p => p.Company)
            .Where(p => p.CompanyId == companyId)
            .AsNoTracking()
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                StockQuantity = p.StockQuantity,
                CategoryId = p.CategoryId,
                CategoryName = p.Category != null ? p.Category.Name : "",
                BrandId = p.BrandId,
                BrandName = p.Brand != null ? p.Brand.Name : "",
                CompanyId = p.CompanyId,
                CompanyName = p.Company != null ? p.Company.Name : "",
                ImageUrl = p.ImageUrl,
                IsActive = p.IsActive,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt,
                ReviewCount = p.Reviews.Count,
                AverageRating = p.Reviews.Any() ? p.Reviews.Average(r => r.Rating) : 0
            })
            .ToListAsync();
        return Ok(products);
    }
    
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll()
    {
        var products = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .Include(p => p.Company)
            .Include(p => p.Reviews)
            .AsNoTracking()
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                StockQuantity = p.StockQuantity,
                CategoryId = p.CategoryId,
                CategoryName = p.Category != null ? p.Category.Name : "",
                BrandId = p.BrandId,
                BrandName = p.Brand != null ? p.Brand.Name : "",
                CompanyId = p.CompanyId,
                CompanyName = p.Company != null ? p.Company.Name : "",
                ImageUrl = p.ImageUrl,
                IsActive = p.IsActive,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt,
                ReviewCount = p.Reviews.Count,
                AverageRating = p.Reviews.Any() ? p.Reviews.Average(r => r.Rating) : 0
            })
            .ToListAsync();
        return Ok(products);
    }
    
    [HttpPut("{id}")]
    [Authorize(Roles = "CompanyAdmin,User,SuperAdmin")]
    public async Task<IActionResult> Update(int id, ProductUpdateDto dto)
    {
        if (id != dto.Id) 
            return BadRequest("Id mismatch");
        
        var product = await _context.Products.FindAsync(id);
        if (product == null)
            return NotFound(new { message = "Ürün bulunamadı" });
        
        product.Update(dto.Name, dto.Description, dto.Price, dto.ImageUrl);
        product.UpdateStock(dto.StockQuantity);
        
        if (dto.IsActive)
            product.Activate();
        else
            product.Deactivate();
        
        await _context.SaveChangesAsync();
        return Ok(new { message = "Ürün Güncellendi" });
    }
    
    [HttpDelete("{id}")]
    [Authorize(Roles = "CompanyAdmin,User,SuperAdmin")]
    public async Task<IActionResult> Delete(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
            return NotFound(new { message = "Ürün bulunamadı" });
        
        _context.Products.Remove(product);
        await _context.SaveChangesAsync();
        
        return Ok(new { message = "Ürün Silindi" });
    }
}