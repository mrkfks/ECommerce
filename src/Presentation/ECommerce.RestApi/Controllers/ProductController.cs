using Microsoft.AspNetCore.Mvc;
using ECommerce.Application.Services;
using ECommerce.Application.DTOs;

namespace ECommerce.RestApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly ProductService _productService;

        public ProductController(ProductService productService)
        {
            _productService = productService;
        }
        //Create
        [HttpPost]
        public async Task<actionResult> Get(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)

                return NotFound(new { message = "Product not found" });
            return Ok(product);
        }
        //READ - Get By Id
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
                return NotFound(new { message = "Product not found" });
            return Ok(product);
        }
        // READ - Get all by Company
        [HttpGet("company/{companyId}")]
        public async Task<IActionResult> GetByCompany(int companyId)
        {
            var products = await _productService.GetByCompanyAsync(companyId);
            return Ok(products);
        }
        //UPDATE
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, ProductDto dto)
        {
            var updated = await _productService.UpdateAsync(id, dto.Name, dto.Price, dto.Stock, dto.CompanyId, dto.CategoryId);
            if(updated == null)
                return NotFound(new { message = "Ürün Güncellenemedi" });
            return Ok(updated);
        }
        //DELETE
        [HttpDelete("{id}")]
        public async Task <IActionResult> Delete(int id)
        {
            var delete = await _productService.DeleteAsync(id);
            if(delete == false)
                return NotFound(new { message = "Ürün Silinemedi" });
            return Ok(new { message = "Ürün Silindi" });
        }

    }

}