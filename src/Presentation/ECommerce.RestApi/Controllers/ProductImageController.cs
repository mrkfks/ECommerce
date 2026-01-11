using ECommerce.Application.DTOs.Common;
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
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITenantService _tenantService;
        private readonly ILogger<ProductImageController> _logger;

        public ProductImageController(IUnitOfWork unitOfWork, ITenantService tenantService, ILogger<ProductImageController> logger)
        {
            _unitOfWork = unitOfWork;
            _tenantService = tenantService;
            _logger = logger;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponseDto<IEnumerable<ProductImageDto>>>> GetProductImages(int productId)
        {
            try
            {
                var product = await _unitOfWork.Products.GetByIdAsync(productId);
                if (product == null)
                {
                    return NotFound(new ApiResponseDto<IEnumerable<ProductImageDto>>
                    {
                        Success = false,
                        Message = "Ürün bulunamadı"
                    });
                }

                var images = product.Images
                    .OrderBy(i => i.Order)
                    .Select(i => new ProductImageDto
                    {
                        Id = i.Id,
                        ProductId = i.ProductId,
                        ImageUrl = i.ImageUrl,
                        Order = i.Order,
                        IsPrimary = i.IsPrimary
                    })
                    .ToList();

                return Ok(new ApiResponseDto<IEnumerable<ProductImageDto>>
                {
                    Success = true,
                    Data = images
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product images for product {ProductId}", productId);
                return StatusCode(500, new ApiResponseDto<IEnumerable<ProductImageDto>>
                {
                    Success = false,
                    Message = "Resimler alınırken hata oluştu"
                });
            }
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponseDto<ProductImageDto>>> AddImage(int productId, [FromBody] AddProductImageDto dto)
        {
            try
            {
                var product = await _unitOfWork.Products.GetByIdAsync(productId);
                if (product == null)
                {
                    return NotFound(new ApiResponseDto<ProductImageDto>
                    {
                        Success = false,
                        Message = "Ürün bulunamadı"
                    });
                }

                var companyId = _tenantService.GetCompanyId();
                if (companyId.HasValue && product.CompanyId != companyId.Value)
                {
                    return Forbid();
                }

                // If setting as primary, unset others
                if (dto.IsPrimary)
                {
                    foreach (var img in product.Images.Where(i => i.IsPrimary))
                    {
                        img.UnsetPrimary();
                    }
                }

                var image = ProductImage.Create(productId, dto.ImageUrl, dto.Order, dto.IsPrimary);
                ((List<ProductImage>)product.Images).Add(image);
                
                await _unitOfWork.SaveChangesAsync();

                return Ok(new ApiResponseDto<ProductImageDto>
                {
                    Success = true,
                    Data = new ProductImageDto
                    {
                        Id = image.Id,
                        ProductId = image.ProductId,
                        ImageUrl = image.ImageUrl,
                        Order = image.Order,
                        IsPrimary = image.IsPrimary
                    },
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
                var product = await _unitOfWork.Products.GetByIdAsync(productId);
                if (product == null)
                {
                    return NotFound(new ApiResponseDto<ProductImageDto>
                    {
                        Success = false,
                        Message = "Ürün bulunamadı"
                    });
                }

                var companyId = _tenantService.GetCompanyId();
                if (companyId.HasValue && product.CompanyId != companyId.Value)
                {
                    return Forbid();
                }

                var image = product.Images.FirstOrDefault(i => i.Id == imageId);
                if (image == null)
                {
                    return NotFound(new ApiResponseDto<ProductImageDto>
                    {
                        Success = false,
                        Message = "Resim bulunamadı"
                    });
                }

                // If setting as primary, unset others
                if (dto.IsPrimary && !image.IsPrimary)
                {
                    foreach (var img in product.Images.Where(i => i.IsPrimary && i.Id != imageId))
                    {
                        img.UnsetPrimary();
                    }
                }

                image.Update(dto.ImageUrl, dto.Order, dto.IsPrimary);
                await _unitOfWork.SaveChangesAsync();

                return Ok(new ApiResponseDto<ProductImageDto>
                {
                    Success = true,
                    Data = new ProductImageDto
                    {
                        Id = image.Id,
                        ProductId = image.ProductId,
                        ImageUrl = image.ImageUrl,
                        Order = image.Order,
                        IsPrimary = image.IsPrimary
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
                var product = await _unitOfWork.Products.GetByIdAsync(productId);
                if (product == null)
                {
                    return NotFound(new ApiResponseDto<bool>
                    {
                        Success = false,
                        Message = "Ürün bulunamadı"
                    });
                }

                var companyId = _tenantService.GetCompanyId();
                if (companyId.HasValue && product.CompanyId != companyId.Value)
                {
                    return Forbid();
                }

                var image = product.Images.FirstOrDefault(i => i.Id == imageId);
                if (image == null)
                {
                    return NotFound(new ApiResponseDto<bool>
                    {
                        Success = false,
                        Message = "Resim bulunamadı"
                    });
                }

                ((List<ProductImage>)product.Images).Remove(image);
                await _unitOfWork.SaveChangesAsync();

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
                    Message = "Resim silinirken hata oluştu"
                });
            }
        }
    }

    public class ProductImageDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public int Order { get; set; }
        public bool IsPrimary { get; set; }
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
