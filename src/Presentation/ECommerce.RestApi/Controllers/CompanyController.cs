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

        public CompanyController(ICompanyService companyService)
        {
            _companyService = companyService;
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
        [Authorize(Policy = "SuperAdminOnly")]
        public async Task<IActionResult> Create([FromBody] CompanyFormDto dto)
        {
            try
            {
                var company = await _companyService.CreateAsync(dto);
                return Ok(new 
                { 
                    message = "Şirket başarıyla kaydedildi.",
                    companyId = company.Id
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Kayıt sırasında hata oluştu: " + ex.Message });
            }
        }

        [HttpGet]
        [Authorize(Policy = "SuperAdminOnly")]
        public async Task<IActionResult> GetAll()
        {
            var companies = await _companyService.GetAllAsync();
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

