using ECommerce.Application.DTOs;

using ECommerce.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.RestApi.Controllers;

[ApiController]
[Route("api/products")]
[Authorize(Policy = "SameCompanyOrSuperAdmin")]
public class ProductController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly ILogger<ProductController> _logger;
    private readonly ITenantService _tenantService;

    public ProductController(IProductService productService, ILogger<ProductController> logger, ITenantService tenantService)
    {
        _productService = productService;
        _logger = logger;
        _tenantService = tenantService;
    }
    
    /// <summary>
    /// Yeni ürün oluşturur
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "CompanyAdmin,User,SuperAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(ProductFormDto dto)
    {
        try
        {
            _logger.LogInformation("Creating new product: {ProductName}", dto.Name);
            var result = await _productService.CreateAsync(dto);
            return Ok(new { Data = result, Message = "Ürün oluşturuldu", Success = true });
        }
        catch (Exception ex)
        {
             return BadRequest(new { message = ex.Message });
        }
    }
    
    /// <summary>
    /// ID'ye göre ürün getirir
    /// </summary>
    [HttpGet("{id}")]
    [AllowAnonymous]
    [ResponseCache(Duration = 60, VaryByQueryKeys = new[] { "id" })]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(int id)
    {
        _logger.LogInformation("Fetching product with ID: {ProductId}", id);
        var result = await _productService.GetByIdAsync(id);
        if (result == null) 
            return NotFound(new ECommerce.Application.Responses.ApiResponse<ProductDto> 
            { 
                Success = false, 
                Message = "Ürün bulunamadı" 
            });
        
        return Ok(new ECommerce.Application.Responses.ApiResponse<ProductDto> 
        { 
            Success = true, 
            Data = result, 
            Message = "" 
        });
    }
    
    /// <summary>
    /// Tüm ürünleri getirir
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    [ResponseCache(Duration = 30, VaryByQueryKeys = new[] { "pageNumber", "pageSize" })]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        _logger.LogInformation("Fetching products (Page {Page}, Size {Size})", pageNumber, pageSize);
        
        var result = await _productService.GetPagedAsync(pageNumber, pageSize);
        
        return Ok(result);
    }

    /// <summary>
    /// Kategoriye göre ürünleri getirir
    /// </summary>
    [HttpGet("category/{categoryId}")]
    [AllowAnonymous]
    [ResponseCache(Duration = 60, VaryByQueryKeys = new[] { "categoryId" })]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByCategory(int categoryId)
    {
        _logger.LogInformation("Fetching products for category: {CategoryId}", categoryId);
        var result = await _productService.GetByCategoryIdAsync(categoryId);
        return Ok(new ECommerce.Application.Responses.ApiResponse<IEnumerable<ProductDto>> 
        { 
            Success = true, 
            Data = result, 
            Message = "" 
        });
    }

    /// <summary>
    /// Ürün arama
    /// </summary>
    [HttpGet("search")]
    [AllowAnonymous]
    [ResponseCache(Duration = 30, VaryByQueryKeys = new[] { "searchTerm" })]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Search([FromQuery] string searchTerm)
    {
        _logger.LogInformation("Searching products with term: {SearchTerm}", searchTerm);
        var result = await _productService.SearchAsync(searchTerm);
        return Ok(new { Data = result, Success = true });
    }

    /// <summary>
    /// Öne çıkan ürünleri getirir
    /// </summary>
    [HttpGet("featured")]
    [AllowAnonymous]
    [ResponseCache(Duration = 60)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFeatured([FromQuery] int count = 8)
    {
        _logger.LogInformation("Fetching featured products, count: {Count}", count);
        var result = await _productService.GetFeaturedAsync(count);
        return Ok(new ECommerce.Application.Responses.ApiResponse<IEnumerable<ProductDto>> 
        { 
            Success = true, 
            Data = result, 
            Message = "" 
        });
    }

    /// <summary>
    /// Yeni ürünleri getirir
    /// </summary>
    [HttpGet("new")]
    [AllowAnonymous]
    [ResponseCache(Duration = 60)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetNewArrivals([FromQuery] int count = 8)
    {
        _logger.LogInformation("Fetching new arrivals, count: {Count}", count);
        var result = await _productService.GetNewArrivalsAsync(count);
        return Ok(new ECommerce.Application.Responses.ApiResponse<IEnumerable<ProductDto>> 
        { 
            Success = true, 
            Data = result, 
            Message = "" 
        });
    }

    /// <summary>
    /// Çok satanları getirir
    /// </summary>
    [HttpGet("bestsellers")]
    [AllowAnonymous]
    [ResponseCache(Duration = 60)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBestSellers([FromQuery] int count = 8)
    {
        _logger.LogInformation("Fetching bestsellers, count: {Count}", count);
        var result = await _productService.GetBestSellersAsync(count);
        return Ok(new ECommerce.Application.Responses.ApiResponse<IEnumerable<ProductDto>> 
        { 
            Success = true, 
            Data = result, 
            Message = "" 
        });
    }
    
    /// <summary>
    /// Ürün günceller
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "CompanyAdmin,User,SuperAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, ProductFormDto dto)
    {
        if (id != dto.Id) 
            return BadRequest(new { message = "Id mismatch" });
        
        try
        {
            _logger.LogInformation("Updating product: {ProductId}", id);
            await _productService.UpdateAsync(dto);
            return Ok(new { message = "Ürün güncellendi", Success = true });
        }
        catch (Exception ex)
        {
             return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Stok günceller
    /// </summary>
    [HttpPatch("{id}/stock")]
    [Authorize(Roles = "CompanyAdmin,User,SuperAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStock(int id, [FromBody] StockUpdateDto dto)
    {
        try
        {
             _logger.LogInformation("Updating stock for product {ProductId} to {StockQuantity}", id, dto.StockQuantity);
             await _productService.UpdateStockAsync(id, dto.StockQuantity);
             return Ok(new { message = "Stok güncellendi", Success = true });
        }
        catch (Exception ex)
        {
             return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Toplu fiyat güncelleme
    /// </summary>
    [HttpPost("bulk-price-update")]
    [Authorize(Roles = "CompanyAdmin,SuperAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> BulkPriceUpdate([FromBody] ProductBulkUpdateDto dto)
    {
        try
        {
            _logger.LogInformation("Bulk updating prices for {Count} products by {Percentage}%", dto.ProductIds.Count, dto.PriceIncreasePercentage);
            await _productService.BulkUpdatePriceAsync(dto.ProductIds, dto.PriceIncreasePercentage);
            return Ok(new { message = "Fiyatlar güncellendi", Success = true });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
    
    /// <summary>
    /// Ürün siler
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "CompanyAdmin,User,SuperAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            _logger.LogInformation("Deleting product: {ProductId}", id);
            await _productService.DeleteAsync(id);
             return Ok(new { message = "Ürün silindi", Success = true });
        }
        catch (Exception ex)
        {
             return NotFound(new { message = ex.Message });
        }
    }
}

public class StockUpdateDto
{
    public int StockQuantity { get; set; }
}