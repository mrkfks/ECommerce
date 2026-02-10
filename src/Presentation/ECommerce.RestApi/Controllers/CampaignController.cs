using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.RestApi.Controllers;

[ApiController]
[Route("api/campaigns")]
[Authorize(Policy = "SameCompanyOrSuperAdmin")]
public class CampaignController : ControllerBase
{
    private readonly ICampaignService _campaignService;
    private readonly IProductCampaignService _productCampaignService;
    private readonly IWebHostEnvironment _env;

    public CampaignController(
        ICampaignService campaignService,
        IProductCampaignService productCampaignService,
        IWebHostEnvironment env)
    {
        _campaignService = campaignService;
        _productCampaignService = productCampaignService;
        _env = env;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var campaigns = await _campaignService.GetAllAsync();
        return Ok(campaigns);
    }

    [HttpGet("active")]
    [AllowAnonymous]  // Müşterilerin aktif kampanyaları görmesi için public
    public async Task<IActionResult> GetActive()
    {
        var campaigns = await _campaignService.GetActiveAsync();
        return Ok(campaigns);
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary()
    {
        var summary = await _campaignService.GetSummaryAsync();
        return Ok(summary);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var campaign = await _campaignService.GetByIdAsync(id);
        if (campaign == null)
            return NotFound(new { message = "Kampanya bulunamadı." });

        return Ok(campaign);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CampaignFormDto dto)
    {
        try
        {
            var campaign = await _campaignService.CreateAsync(dto);
            return Ok(new { id = campaign.Id, message = "Kampanya başarıyla oluşturuldu." });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] CampaignFormDto dto)
    {
        try
        {
            await _campaignService.UpdateAsync(id, dto);
            return Ok(new { message = "Kampanya başarıyla güncellendi." });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Kampanya bulunamadı." });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}/activate")]
    public async Task<IActionResult> Activate(int id)
    {
        try
        {
            await _campaignService.ActivateAsync(id);
            return Ok(new { message = "Kampanya aktifleştirildi." });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Kampanya bulunamadı." });
        }
    }

    [HttpPut("{id}/deactivate")]
    public async Task<IActionResult> Deactivate(int id)
    {
        try
        {
            await _campaignService.DeactivateAsync(id);
            return Ok(new { message = "Kampanya pasifleştirildi." });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Kampanya bulunamadı." });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _campaignService.DeleteAsync(id);
            return Ok(new { success = true, message = "Kampanya başarıyla silindi." });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message ?? "Kampanya bulunamadı." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, message = ex.Message ?? "Kampanya silinemedi." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "Kampanya silinirken bir hata oluştu: " + ex.Message });
        }
    }

    [HttpPut("{id}/upload-banner")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadBanner(int id, IFormFile file)
    {
        try
        {
            // Validation
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "Dosya boş veya seçilmemiş." });

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var fileExtension = Path.GetExtension(file.FileName).ToLower();

            if (!allowedExtensions.Contains(fileExtension))
                return BadRequest(new { message = "Yalnızca resim dosyaları (jpg, png, gif, webp) yüklenebilir." });

            if (file.Length > 5 * 1024 * 1024) // 5 MB limit
                return BadRequest(new { message = "Dosya boyutu 5 MB'ı aşamaz." });

            // Get campaign to verify it exists
            var campaign = await _campaignService.GetByIdAsync(id);
            if (campaign == null)
                return NotFound(new { message = "Kampanya bulunamadı." });

            // Create campaigns uploads directory - handle missing WebRootPath
            var webRootPath = _env.WebRootPath;
            if (string.IsNullOrEmpty(webRootPath))
            {
                webRootPath = Path.Combine(_env.ContentRootPath, "wwwroot");
                Directory.CreateDirectory(webRootPath);
            }

            var uploadsDir = Path.Combine(webRootPath, "uploads", "campaigns");
            Directory.CreateDirectory(uploadsDir);

            // Generate unique filename
            var fileName = $"{id}_{Guid.NewGuid()}{fileExtension}";
            var filePath = Path.Combine(uploadsDir, fileName);

            // Save file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Update campaign with relative URL - PRESERVE ALL EXISTING VALUES including DiscountPercent
            var relativePath = $"/uploads/campaigns/{fileName}";
            var updateDto = new CampaignFormDto
            {
                Name = campaign.Name,
                Description = campaign.Description,
                DiscountPercent = campaign.DiscountPercent,  // FIX: Include DiscountPercent
                StartDate = campaign.StartDate,
                EndDate = campaign.EndDate,
                BannerImageUrl = relativePath
            };

            await _campaignService.UpdateAsync(id, updateDto);

            return Ok(new
            {
                message = "Banner başarıyla yüklendi.",
                url = relativePath,
                data = campaign
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = $"Banner yükleme hatası: {ex.Message}" });
        }
    }

    // ProductCampaign Endpoints

    [HttpGet("{campaignId}/products")]
    public async Task<IActionResult> GetCampaignProducts(int campaignId)
    {
        var products = await _productCampaignService.GetByCampaignIdAsync(campaignId);
        return Ok(products);
    }

    [HttpPost("{campaignId}/products")]
    public async Task<IActionResult> AddProductToCampaign(int campaignId, [FromBody] ProductCampaignFormDto dto)
    {
        try
        {
            var newDto = new ProductCampaignFormDto
            {
                ProductId = dto.ProductId,
                CampaignId = campaignId,
                OriginalPrice = dto.OriginalPrice,
                DiscountedPrice = dto.DiscountedPrice
            };
            var result = await _productCampaignService.AddProductToCampaignAsync(newDto);
            return Ok(new { data = result, message = "Ürün kampanyaya başarıyla eklendi." });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpDelete("{campaignId}/products/{productId}")]
    public async Task<IActionResult> RemoveProductFromCampaign(int campaignId, int productId)
    {
        try
        {
            await _productCampaignService.RemoveProductFromCampaignAsync(productId, campaignId);
            return Ok(new { message = "Ürün kampanyadan başarıyla kaldırıldı." });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPut("{campaignId}/products/{productId}")]
    public async Task<IActionResult> UpdateProductCampaignPrices(int campaignId, int productId, [FromBody] ProductCampaignFormDto dto)
    {
        try
        {
            var newDto = new ProductCampaignFormDto
            {
                ProductId = productId,
                CampaignId = campaignId,
                OriginalPrice = dto.OriginalPrice,
                DiscountedPrice = dto.DiscountedPrice
            };
            await _productCampaignService.UpdatePricesAsync(productId, campaignId, newDto);
            return Ok(new { message = "Kampanya fiyatları başarıyla güncellendi." });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    // Category-based Campaign Endpoints

    /// <summary>
    /// Get all category IDs associated with a campaign
    /// </summary>
    [HttpGet("{campaignId}/categories")]
    public async Task<IActionResult> GetCampaignCategories(int campaignId)
    {
        var categoryIds = await _campaignService.GetCampaignCategoryIdsAsync(campaignId);
        return Ok(categoryIds);
    }

    /// <summary>
    /// Add multiple categories to a campaign
    /// </summary>
    [HttpPost("{campaignId}/categories")]
    public async Task<IActionResult> AddCategoriesToCampaign(int campaignId, [FromBody] CategoryIdsRequest request)
    {
        try
        {
            await _campaignService.AddCategoriesToCampaignAsync(campaignId, request.CategoryIds);
            return Ok(new { message = $"{request.CategoryIds.Count} kategori kampanyaya eklendi." });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Remove a category from a campaign
    /// </summary>
    [HttpDelete("{campaignId}/categories/{categoryId}")]
    public async Task<IActionResult> RemoveCategoryFromCampaign(int campaignId, int categoryId)
    {
        try
        {
            await _campaignService.RemoveCategoryFromCampaignAsync(campaignId, categoryId);
            return Ok(new { message = "Kategori kampanyadan kaldırıldı." });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}

/// <summary>
/// Request model for adding categories to campaign
/// </summary>
public record CategoryIdsRequest(List<int> CategoryIds);
