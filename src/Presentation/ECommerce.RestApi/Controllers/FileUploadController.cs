
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
        private readonly ICompanyService _companyService;
        private readonly ILogger<FileUploadController> _logger;

        public FileUploadController(
            IFileUploadService fileUploadService,
            IProductService productService,
            ICategoryService categoryService,
            IBrandService brandService,
            IBannerService bannerService,
            ICompanyService companyService,
            ILogger<FileUploadController> logger)
        {
            _fileUploadService = fileUploadService;
            _productService = productService;
            _categoryService = categoryService;
            _brandService = brandService;
            _bannerService = bannerService;
            _companyService = companyService;
            _logger = logger;
        }

        // Upload Product Image
        [HttpPost("product/{productId}")]
        [Authorize]
        public async Task<IActionResult> UploadProductImage(int productId, IFormFile file)
        {
            _logger.LogInformation("Upload request received for product {ProductId}", productId);
            
            if (file == null || file.Length == 0)
            {
                _logger.LogWarning("No file provided in upload request");
                return BadRequest(new { success = false, message = "Lütfen geçerli bir dosya seçin." });
            }

            _logger.LogInformation("File received: {FileName}, Size: {Size} bytes", file.FileName, file.Length);
            
            try
            {
                using (var memoryStream = new MemoryStream())
                {
                    await file.CopyToAsync(memoryStream);
                    var fileBytes = memoryStream.ToArray();
                    
                    _logger.LogInformation("Uploading image to storage...");
                    var imageUrl = await _fileUploadService.UploadImageAsync(fileBytes, file.FileName, "products");
                    _logger.LogInformation("Image uploaded successfully: {ImageUrl}", imageUrl);
                    
                    // Add as not primary by default
                    await _productService.AddImageAsync(productId, imageUrl, 0, false);
                    _logger.LogInformation("Image added to product {ProductId}", productId);
                    
                    return Ok(new { success = true, imageUrl = imageUrl, message = "Ürün resmi yüklendi" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ürün resmi yükleme hatası");
                return StatusCode(500, new { success = false, message = "Dosya yüklenirken hata oluştu: " + ex.Message });
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

        // Upload Company Logo
        [HttpPost("company/{companyId}")]
        [Authorize(Policy = "SuperAdminOnly")]
        public async Task<IActionResult> UploadCompanyLogo(int companyId, IFormFile file)
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
                    
                    var imageUrl = await _fileUploadService.UploadImageAsync(fileBytes, file.FileName, "companies");
                    
                    await _companyService.UpdateLogoAsync(companyId, imageUrl);
                    
                    return Ok(new ApiResponseDto<string> { Success = true, Data = imageUrl, Message = "Şirket logosu yüklendi" });
                }
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "Şirket bulunamadı" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Şirket logosu yükleme hatası");
                return StatusCode(500, new { message = "Dosya yüklenirken hata oluştu: " + ex.Message });
            }
        }
    }
}

