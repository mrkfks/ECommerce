using System;
using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.RestApi.Controllers
{
    [ApiController]
    [Route("api/companies")]
    public class CompanyController : ControllerBase
    {
        private readonly ICompanyService _companyService;
        private readonly ILogger<CompanyController> _logger;

        public CompanyController(ICompanyService companyService, ILogger<CompanyController> logger)
        {
            _companyService = companyService;
            _logger = logger;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            try
            {
                var result = await _companyService.RegisterCompanyAsync(dto);
                return Ok(new
                {
                    message = result.Message,
                    companyId = result.CompanyId,
                    companyName = result.CompanyName,
                    username = result.Username
                });
            }
            catch (Exception ex)
            {
                // In production, don't expose internal exception details unless safe
                return BadRequest(new { message = "Kayıt sırasında hata oluştu: " + ex.Message });
            }
        }

        [HttpPost]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> Create([FromBody] CompanyFormDto dto)
        {
            _logger.LogInformation("[CompanyController.Create] Request received - Name: {Name}, Email: {Email}", dto?.Name, dto?.Email);

            try
            {
                if (dto == null)
                {
                    _logger.LogWarning("[CompanyController.Create] DTO is null");
                    return BadRequest(new { success = false, message = "Geçersiz veri" });
                }

                var company = await _companyService.CreateAsync(dto);
                _logger.LogInformation("[CompanyController.Create] Company created successfully - ID: {Id}", company.Id);

                return Ok(new
                {
                    success = true,
                    message = "Şirket başarıyla kaydedildi.",
                    companyId = company.Id
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[CompanyController.Create] Exception occurred");
                return BadRequest(new { success = false, data = (object?)null, message = ex.Message });
            }
        }

        [HttpGet]
        [Authorize(Roles = "SuperAdmin,CompanyAdmin")]
        public async Task<IActionResult> GetAll()
        {
            _logger.LogInformation("[CompanyController.GetAll] START - User: {User}, Roles: {Roles}", User.Identity?.Name, string.Join(",", User.Claims.Where(c => c.Type == System.Security.Claims.ClaimTypes.Role).Select(c => c.Value)));

            var companies = await _companyService.GetAllAsync();

            _logger.LogInformation("[CompanyController.GetAll] Service returned {Count} companies", companies?.Count() ?? 0);

            if (companies != null)
            {
                foreach (var c in companies)
                {
                    _logger.LogInformation("[CompanyController.GetAll] Company: Id={Id}, Name={Name}, Email={Email}, IsActive={IsActive}, IsApproved={IsApproved}",
                        c.Id, c.Name, c.Email, c.IsActive, c.IsApproved);
                }
            }

            return Ok(companies);
        }

        [HttpGet("{id:int}")]
        [Authorize(Policy = "SuperAdminOnly")]
        public async Task<IActionResult> GetById(int id)
        {
            var company = await _companyService.GetByIdAsync(id);

            if (company == null)
                return NotFound(new { message = "Şirket bulunamadı" });

            return Ok(company);
        }

        [HttpPut("{id:int}")]
        [Authorize(Policy = "SuperAdminOnly")]
        public async Task<IActionResult> Update(int id, [FromBody] CompanyFormDto dto)
        {
            if (id != dto.Id)
                return BadRequest(new { message = "ID uyuşmazlığı" });

            try
            {
                await _companyService.UpdateAsync(id, dto);
                return Ok(new { message = "Şirket bilgileri güncellendi" });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "Şirket bulunamadı" });
            }
        }

        [HttpPost("{id:int}/approve")]
        [Authorize(Policy = "SuperAdminOnly")]
        public async Task<IActionResult> Approve(int id)
        {
            try
            {
                await _companyService.ApproveAsync(id);
                return Ok(new { message = "Şirket başarıyla onaylandı" });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "Şirket bulunamadı" });
            }
        }

        [HttpPost("{id:int}/reject")]
        [Authorize(Policy = "SuperAdminOnly")]
        public async Task<IActionResult> Reject(int id)
        {
            try
            {
                await _companyService.RejectAsync(id);
                return Ok(new { message = "Şirket onayı reddedildi" });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "Şirket bulunamadı" });
            }
        }

        [HttpPost("{id:int}/deactivate")]
        [Authorize(Policy = "SuperAdminOnly")]
        public async Task<IActionResult> Deactivate(int id)
        {
            try
            {
                await _companyService.DeactivateAsync(id);
                return Ok(new { message = "Şirket pasifleştirildi" });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "Şirket bulunamadı" });
            }
        }

        [HttpPost("{id:int}/activate")]
        [Authorize(Policy = "SuperAdminOnly")]
        public async Task<IActionResult> Activate(int id)
        {
            try
            {
                await _companyService.ActivateAsync(id);
                return Ok(new { message = "Şirket aktif hale getirildi" });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "Şirket bulunamadı" });
            }
        }
        [HttpGet("settings")]
        [HttpGet("/api/company/settings")]
        [AllowAnonymous]
        public async Task<IActionResult> GetSettings([FromQuery] string domain)
        {
            if (string.IsNullOrWhiteSpace(domain))
                return BadRequest(new { message = "Domain parametres zorunludur" });

            try
            {
                var company = await _companyService.GetByDomainAsync(domain);
                if (company == null)
                    return NotFound(new { message = "Bu domain için şirket bulunamadı" });

                return Ok(new
                {
                    id = company.Id,
                    companyName = company.Name,
                    logoUrl = company.LogoUrl,
                    primaryColor = company.PrimaryColor,
                    secondaryColor = company.SecondaryColor,
                    faviconUrl = company.LogoUrl, // Use logo as favicon if no specific favicon
                    isActive = company.IsActive,
                    isApproved = company.IsApproved,
                    domain = company.Domain
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetSettings: {ex.Message} \nStack Trace: {ex.StackTrace}");
                return BadRequest(new { message = "Ayarlar alınırken hata oluştu: " + ex.Message });
            }
        }


        [HttpPost("{id:int}/upload-logo")]
        [Authorize(Policy = "SuperAdminOnly")]
        public async Task<IActionResult> UploadLogo(int id, IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest(new { message = "Dosya boş" });

                // Dosya validasyonu
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".svg" };
                var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(fileExtension))
                    return BadRequest(new { message = "Desteklenmeyen dosya formatı. Lütfen JPG, PNG, GIF veya SVG dosyası yükleyin." });

                // Dosya boyutu validasyonu (2MB)
                const long maxFileSize = 2 * 1024 * 1024;
                if (file.Length > maxFileSize)
                    return BadRequest(new { message = "Dosya 2MB'dan büyük olamaz." });

                // Dosya yükleme
                var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "logos");
                Directory.CreateDirectory(uploadsPath);

                var fileName = $"{id}_{Guid.NewGuid()}{fileExtension}";
                var filePath = Path.Combine(uploadsPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var logoUrl = $"/uploads/logos/{fileName}";

                // Veritabanında güncelle
                await _companyService.UpdateLogoAsync(id, logoUrl);

                _logger.LogInformation("[CompanyController.UploadLogo] Logo uploaded successfully for company {Id}: {LogoUrl}", id, logoUrl);

                return Ok(new { message = "Logo başarıyla yüklendi", logoUrl });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "Şirket bulunamadı" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[CompanyController.UploadLogo] Error uploading logo for company {Id}", id);
                return BadRequest(new { message = $"Logo yükleme hatası: {ex.Message}" });
            }
        }

        [HttpPut("{id:int}/branding")]
        [Authorize(Policy = "SuperAdminOnly")]
        public async Task<IActionResult> UpdateBranding(int id, [FromBody] BrandingUpdateDto dto)
        {
            try
            {
                await _companyService.UpdateBrandingAsync(id, dto);
                return Ok(new { message = "Marka bilgileri güncellendi" });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "Şirket bulunamadı" });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }
    }
}

