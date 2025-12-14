using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.RestApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "SameCompanyOrSuperAdmin")]
public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }
        //Create
        [HttpPost]
        [Authorize(Roles = "CompanyAdmin,SuperAdmin")]
        public async Task<IActionResult> Create(ProductCreateDto dto)
        {
            var product = await _productService.CreateAsync(dto);
            return Ok(product);
        }
        //READ - Get By Id
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> Get(int id)
        {
            var product = await _productService.GetByIdAsync(id);
            if (product == null)
                return NotFound(new { message = "Product not found" });
            return Ok(product);
        }
        // READ - Get all by Company
        [HttpGet("company/{companyId}")]
        [Authorize]
        public async Task<IActionResult> GetByCompany(int companyId)
        {
            var products = await _productService.GetByCompanyAsync(companyId);
            return Ok(products);
        }
        // GET: api/product
        [HttpGet]
        [AllowAnonymous] // Ürünleri herkese göster
        public async Task<IActionResult> GetAll()
        {
            var products = await _productService.GetAllAsync();
            return Ok(products);
        }
        //UPDATE
        [HttpPut("{id}")]
        [Authorize(Roles = "CompanyAdmin,SuperAdmin")]
        public async Task<IActionResult> Update(int id, ProductUpdateDto dto)
        {
            if (id != dto.Id) return BadRequest("Id mismatch");
            
            await _productService.UpdateAsync(dto);
            return Ok(new { message = "Ürün Güncellendi" });
        }
        //DELETE
        [HttpDelete("{id}")]
        [Authorize(Roles = "CompanyAdmin,SuperAdmin")]
        public async Task<IActionResult> Delete(int id)
        {
            await _productService.DeleteAsync(id);
            return Ok(new { message = "Ürün Silindi" });
        }
}