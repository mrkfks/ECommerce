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
            return Ok(new { message = "Kampanya silindi." });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Kampanya bulunamadı." });
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

            // Create campaigns uploads directory if it doesn't exist
            var uploadsDir = Path.Combine(_env.WebRootPath, "uploads", "campaigns");
            Directory.CreateDirectory(uploadsDir);

            // Generate unique filename
            var fileName = $"{id}_{Guid.NewGuid()}{fileExtension}";
            var filePath = Path.Combine(uploadsDir, fileName);

            // Save file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Update campaign with relative URL
            var relativePath = $"/uploads/campaigns/{fileName}";
            var updateDto = new CampaignFormDto
            {
                Name = campaign.Name,
                Description = campaign.Description,
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
}
