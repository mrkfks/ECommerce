
using ECommerce.Application.Features.Category.Commands;
using ECommerce.Application.Features.Brand.Commands;
using ECommerce.Application.Features.Banner.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.RestApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FileUploadController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<FileUploadController> _logger;

        public FileUploadController(IMediator mediator, ILogger<FileUploadController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        // Upload Product Image
        [HttpPost("product/{productId}")]
        [Authorize]
        public async Task<IActionResult> UploadProductImage(int productId, IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { message = "Lütfen geçerli bir dosya seçin." });
            }

            try
            {
                using (var stream = file.OpenReadStream())
                {
                    var command = new ECommerce.Application.Features.Products.Commands.UploadProductImageCommand
                    {
                        ProductId = productId,
                        FileStream = stream,
                        FileName = file.FileName,
                        IsPrimary = false // Default
                    };

                    var result = await _mediator.Send(command);

                    if (result.Success)
                    {
                        return Ok(result);
                    }

                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ürün resmi yükleme hatası");
                return StatusCode(500, new { message = "Dosya yüklenirken hata oluştu." });
            }
        }

        // Upload Category Image
        [HttpPost("category/{categoryId}")]
        [Authorize]
        public async Task<IActionResult> UploadCategoryImage(int categoryId, IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { message = "Lütfen geçerli bir dosya seçin." });
            }

            try
            {
                // IFormFile'ı byte array'e dönüştür
                using (var memoryStream = new MemoryStream())
                {
                    await file.CopyToAsync(memoryStream);
                    var fileBytes = memoryStream.ToArray();

                    var command = new UploadCategoryImageCommand
                    {
                        CategoryId = categoryId,
                        ImageFileBytes = fileBytes,
                        FileName = file.FileName
                    };

                    var result = await _mediator.Send(command);

                    if (result.Success)
                    {
                        return Ok(result);
                    }

                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kategori resmi yükleme hatası");
                return StatusCode(500, new { message = "Dosya yüklenirken hata oluştu." });
            }
        }

        // Upload Brand Image
        [HttpPost("brand/{brandId}")]
        [Authorize]
        public async Task<IActionResult> UploadBrandImage(int brandId, IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { message = "Lütfen geçerli bir dosya seçin." });
            }

            try
            {
                // IFormFile'ı byte array'e dönüştür
                using (var memoryStream = new MemoryStream())
                {
                    await file.CopyToAsync(memoryStream);
                    var fileBytes = memoryStream.ToArray();

                    var command = new UploadBrandImageCommand
                    {
                        BrandId = brandId,
                        ImageFileBytes = fileBytes,
                        FileName = file.FileName
                    };

                    var result = await _mediator.Send(command);

                    if (result.Success)
                    {
                        return Ok(result);
                    }

                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Marka resmi yükleme hatası");
                return StatusCode(500, new { message = "Dosya yüklenirken hata oluştu." });
            }
        }

        // Upload Banner Image
        [HttpPost("banner/{bannerId}")]
        [Authorize]
        public async Task<IActionResult> UploadBannerImage(int bannerId, IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { message = "Lütfen geçerli bir dosya seçin." });
            }

            try
            {
                // IFormFile'ı byte array'e dönüştür
                using (var memoryStream = new MemoryStream())
                {
                    await file.CopyToAsync(memoryStream);
                    var fileBytes = memoryStream.ToArray();

                    var command = new UploadBannerImageCommand
                    {
                        BannerId = bannerId,
                        ImageFileBytes = fileBytes,
                        FileName = file.FileName
                    };

                    var result = await _mediator.Send(command);

                    if (result.Success)
                    {
                        return Ok(result);
                    }

                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Banner resmi yükleme hatası");
                return StatusCode(500, new { message = "Dosya yüklenirken hata oluştu." });
            }
        }
    }
}
