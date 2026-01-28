using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ECommerce.RestApi.Controllers
{
    [ApiController]
    [Route("api/product/{productId}/images")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class ProductImageController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly ITenantService _tenantService;
        private readonly ILogger<ProductImageController> _logger;

        public ProductImageController(IProductService productService, ITenantService tenantService, ILogger<ProductImageController> logger)
        {
            _productService = productService;
            _tenantService = tenantService;
            _logger = logger;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponseDto<IEnumerable<ProductImageDto>>>> GetProductImages(int productId)
        {
            try
            {
                var images = await _productService.GetImagesAsync(productId);
                return Ok(new ApiResponseDto<IEnumerable<ProductImageDto>>
                {
                    Success = true,
                    Data = images
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product images for product {ProductId}", productId);
                return NotFound(new ApiResponseDto<IEnumerable<ProductImageDto>>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponseDto<ProductImageDto>>> AddImage(int productId, [FromBody] AddProductImageDto dto)
        {
            try
            {
                var image = await _productService.AddImageAsync(productId, dto.ImageUrl, dto.Order, dto.IsPrimary);
                return Ok(new ApiResponseDto<ProductImageDto>
                {
                    Success = true,
                    Data = image,
                    Message = "Resim başarıyla eklendi"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding image to product {ProductId}", productId);
                return StatusCode(500, new ApiResponseDto<ProductImageDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpPut("{imageId}")]
        public async Task<ActionResult<ApiResponseDto<ProductImageDto>>> UpdateImage(int productId, int imageId, [FromBody] UpdateProductImageDto dto)
        {
            try
            {
                await _productService.UpdateImageAsync(productId, imageId, dto.ImageUrl, dto.Order, dto.IsPrimary);
                // Return updated image DTO (manual construction as UpdateImageAsync returns void)
                return Ok(new ApiResponseDto<ProductImageDto>
                {
                    Success = true,
                    Data = new ProductImageDto
                    {
                        Id = imageId,
                        ProductId = productId,
                        ImageUrl = dto.ImageUrl,
                        Order = dto.Order,
                        IsPrimary = dto.IsPrimary
                    },
                    Message = "Resim başarıyla güncellendi"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating image {ImageId} for product {ProductId}", imageId, productId);
                return StatusCode(500, new ApiResponseDto<ProductImageDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpDelete("{imageId}")]
        public async Task<ActionResult<ApiResponseDto<bool>>> DeleteImage(int productId, int imageId)
        {
            try
            {
                await _productService.RemoveImageAsync(productId, imageId);
                return Ok(new ApiResponseDto<bool>
                {
                    Success = true,
                    Data = true,
                    Message = "Resim başarıyla silindi"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting image {ImageId} for product {ProductId}", imageId, productId);
                return StatusCode(500, new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }
    }

    public class AddProductImageDto
    {
        public string ImageUrl { get; set; } = string.Empty;
        public int Order { get; set; }
        public bool IsPrimary { get; set; }
    }

    public class UpdateProductImageDto
    {
        public string ImageUrl { get; set; } = string.Empty;
        public int Order { get; set; }
        public bool IsPrimary { get; set; }
    }
}
