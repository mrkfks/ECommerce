
using ECommerce.Application.Interfaces;
using ECommerce.Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.RestApi.Controllers
{
    [ApiController]
    [Route("api/files")]
    public class FileUploadController : ControllerBase
    {
        private readonly IFileUploadService _fileUploadService;
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IBrandService _brandService;
        private readonly IBannerService _bannerService;
        private readonly ILogger<FileUploadController> _logger;

        public FileUploadController(
            IFileUploadService fileUploadService,
            IProductService productService,
            ICategoryService categoryService,
            IBrandService brandService,
            IBannerService bannerService,
            ILogger<FileUploadController> logger)
        {
            _fileUploadService = fileUploadService;
            _productService = productService;
            _categoryService = categoryService;
            _brandService = brandService;
            _bannerService = bannerService;
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
                using (var memoryStream = new MemoryStream())
                {
                    await file.CopyToAsync(memoryStream);
                    var fileBytes = memoryStream.ToArray();
                    
                    var imageUrl = await _fileUploadService.UploadImageAsync(fileBytes, file.FileName, "products");
                    
                    // Add as not primary by default
                    var result = await _productService.AddImageAsync(productId, imageUrl, 0, false);
                    
                    return Ok(new ApiResponseDto<object> { Success = true, Data = result, Message = "Ürün resmi yüklendi" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ürün resmi yükleme hatası");
                return StatusCode(500, new { message = "Dosya yüklenirken hata oluştu: " + ex.Message });
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
                 using (var memoryStream = new MemoryStream())
                {
                    await file.CopyToAsync(memoryStream);
                    var fileBytes = memoryStream.ToArray();
                    
                    var imageUrl = await _fileUploadService.UploadImageAsync(fileBytes, file.FileName, "categories");
                    
                    await _categoryService.UpdateImageAsync(categoryId, imageUrl);
                    
                    return Ok(new ApiResponseDto<string> { Success = true, Data = imageUrl, Message = "Kategori resmi yüklendi" });
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
                using (var memoryStream = new MemoryStream())
                {
                    await file.CopyToAsync(memoryStream);
                    var fileBytes = memoryStream.ToArray();
                    
                    var imageUrl = await _fileUploadService.UploadImageAsync(fileBytes, file.FileName, "brands");
                    
                    await _brandService.UpdateImageAsync(brandId, imageUrl);
                    
                    return Ok(new ApiResponseDto<string> { Success = true, Data = imageUrl, Message = "Marka resmi yüklendi" });
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
                using (var memoryStream = new MemoryStream())
                {
                    await file.CopyToAsync(memoryStream);
                    var fileBytes = memoryStream.ToArray();
                    
                    var imageUrl = await _fileUploadService.UploadImageAsync(fileBytes, file.FileName, "banners");
                    
                    var result = await _bannerService.UpdateImageAsync(bannerId, imageUrl);
                    
                    if (result.Success)
                         return Ok(new ApiResponseDto<string> { Success = true, Data = imageUrl, Message = "Banner resmi yüklendi" });
                    else
                         return BadRequest(new { message = result.Message });
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
