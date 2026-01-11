using ECommerce.Application.DTOs;
using ECommerce.Application.DTOs.Common;
using ECommerce.Application.Features.Products.Commands;
using ECommerce.Application.Features.Products.Queries;
using ECommerce.Application.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.RestApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "SameCompanyOrSuperAdmin")]
public class ProductController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ProductController> _logger;
    private readonly ITenantService _tenantService;

    public ProductController(IMediator mediator, ILogger<ProductController> logger, ITenantService tenantService)
    {
        _mediator = mediator;
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
    public async Task<IActionResult> Create(ProductCreateDto dto)
    {
        _logger.LogInformation("Creating new product: {ProductName}", dto.Name);
        var command = new CreateProductCommand { Product = dto };
        var result = await _mediator.Send(command);
        return Ok(result);
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
        var query = new GetProductByIdQuery { Id = id };
        var result = await _mediator.Send(query);
        return Ok(result);
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
        var currentCompanyId = _tenantService.GetCompanyId();
        _logger.LogInformation("Fetching products (Page {Page}, Size {Size}) - Current CompanyId: {CompanyId}", pageNumber, pageSize, currentCompanyId?.ToString() ?? "NULL");
        var query = new GetAllProductsQuery { PageNumber = pageNumber, PageSize = pageSize };
        var result = await _mediator.Send(query);
        // Result is now ApiResponseDto<PaginatedResult<ProductDto>>
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
        var query = new GetProductsByCategoryQuery { CategoryId = categoryId };
        var result = await _mediator.Send(query);
        return Ok(result);
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
        var query = new SearchProductsQuery { SearchTerm = searchTerm };
        var result = await _mediator.Send(query);
        return Ok(result);
    }
    
    /// <summary>
    /// Ürün günceller
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "CompanyAdmin,User,SuperAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, ProductUpdateDto dto)
    {
        if (id != dto.Id) 
            return BadRequest(new { message = "Id mismatch" });
        
        _logger.LogInformation("Updating product: {ProductId}", id);
        var command = new UpdateProductCommand { Id = id, Product = dto };
        var result = await _mediator.Send(command);
        return Ok(result);
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
        _logger.LogInformation("Updating stock for product {ProductId} to {StockQuantity}", id, dto.StockQuantity);
        var command = new UpdateStockCommand { ProductId = id, StockQuantity = dto.StockQuantity };
        var result = await _mediator.Send(command);
        return Ok(result);
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
        _logger.LogInformation("Deleting product: {ProductId}", id);
        var command = new DeleteProductCommand { Id = id };
        var result = await _mediator.Send(command);
        return Ok(result);
    }
}