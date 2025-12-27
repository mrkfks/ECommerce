using ECommerce.Infrastructure.Data;
using ECommerce.Application.DTOs;
using ECommerce.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.RestApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CompanyController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CompanyController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Email ve Username kontrolleri (TÜM ŞİRKETLERDE benzersiz)
                var existingCompany = await _context.Companies
                    .FirstOrDefaultAsync(c => c.Email == dto.CompanyEmail);
                
                if (existingCompany != null)
                {
                    return BadRequest(new { message = "Bu email adresi ile kayıtlı bir şirket zaten mevcut." });
                }

                var existingUser = await _context.Users
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(u => u.Email == dto.Email || u.Username == dto.Username);
                
                if (existingUser != null)
                {
                    if (existingUser.Email == dto.Email)
                        return BadRequest(new { message = "Bu email adresi ile kayıtlı bir kullanıcı zaten mevcut." });
                    else
                        return BadRequest(new { message = "Bu kullanıcı adı zaten kullanılıyor." });
                }

                // Yeni şirket oluştur (IsApproved = false)
                var company = Company.Create(
                    dto.CompanyName,
                    dto.CompanyAddress,
                    dto.CompanyPhoneNumber,
                    dto.CompanyEmail,
                    dto.TaxNumber,
                    $"{dto.FirstName} {dto.LastName}", // Sorumlu kişi adı
                    dto.CompanyPhoneNumber, // Sorumlu telefon (varsayılan olarak şirket telefonu)
                    dto.Email // Sorumlu email (yetkili yöneticinin emaili)
                );

                _context.Companies.Add(company);
                await _context.SaveChangesAsync();

                // Admin kullanıcı oluştur
                var adminUser = Domain.Entities.User.Create(
                    company.Id,
                    dto.Username,
                    dto.Email,
                    BCrypt.Net.BCrypt.HashPassword(dto.Password),
                    dto.FirstName,
                    dto.LastName
                );

                _context.Users.Add(adminUser);
                await _context.SaveChangesAsync();

                // CompanyAdmin rolü ata
                var companyAdminRole = await _context.Roles
                    .FirstOrDefaultAsync(r => r.Name == "CompanyAdmin");
                
                if (companyAdminRole != null)
                {
                    var userRole = UserRole.Create(adminUser.Id, companyAdminRole.Id, companyAdminRole.Name);
                    _context.UserRoles.Add(userRole);
                    await _context.SaveChangesAsync();
                }

                await transaction.CommitAsync();

                return Ok(new { 
                    message = "Şirket ve kullanıcı kaydınız alınmıştır. Süper admin onayından sonra giriş yapabileceksiniz.",
                    companyId = company.Id,
                    companyName = company.Name,
                    username = adminUser.Username
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return BadRequest(new { message = "Kayıt sırasında hata oluştu: " + ex.Message });
            }
        }

        [HttpPost]
        [Authorize(Policy = "SuperAdminOnly")]
        public async Task<IActionResult> Create([FromBody] CompanyDto dto)
        {
            try
            {
                // Email kontrolü
                var existingCompany = await _context.Companies
                    .FirstOrDefaultAsync(c => c.Email == dto.Email);
                
                if (existingCompany != null)
                {
                    return BadRequest(new { message = "Bu email adresi ile kayıtlı bir şirket zaten mevcut." });
                }

                // Yeni şirket oluştur (otomatik IsApproved = false)
                var company = Company.Create(
                    dto.Name,
                    dto.Address,
                    dto.PhoneNumber,
                    dto.Email,
                    dto.TaxNumber,
                    dto.ResponsiblePersonName,
                    dto.ResponsiblePersonPhone,
                    dto.ResponsiblePersonEmail
                );

                _context.Companies.Add(company);
                await _context.SaveChangesAsync();

                return Ok(new { 
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
            var companies = await _context.Companies
                .AsNoTracking()
                .Select(c => new CompanyDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Address = c.Address,
                    PhoneNumber = c.PhoneNumber,
                    Email = c.Email,
                    TaxNumber = c.TaxNumber,
                    ResponsiblePersonName = c.ResponsiblePersonName,
                    ResponsiblePersonPhone = c.ResponsiblePersonPhone,
                    ResponsiblePersonEmail = c.ResponsiblePersonEmail,
                    IsActive = c.IsActive,
                    IsApproved = c.IsApproved,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt,
                    UserCount = c.Users.Count,
                    CustomerCount = c.Customers.Count
                })
                .ToListAsync();
            return Ok(companies);
        }

        [HttpGet("{id:int}")]
        [Authorize(Policy = "SuperAdminOnly")]
        public async Task<IActionResult> GetById(int id)
        {
            var company = await _context.Companies
                .AsNoTracking()
                .Select(c => new CompanyDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Address = c.Address,
                    PhoneNumber = c.PhoneNumber,
                    Email = c.Email,
                    TaxNumber = c.TaxNumber,
                    ResponsiblePersonName = c.ResponsiblePersonName,
                    ResponsiblePersonPhone = c.ResponsiblePersonPhone,
                    ResponsiblePersonEmail = c.ResponsiblePersonEmail,
                    IsActive = c.IsActive,
                    IsApproved = c.IsApproved,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt,
                    UserCount = c.Users.Count,
                    CustomerCount = c.Customers.Count
                })
                .FirstOrDefaultAsync(c => c.Id == id);
            
            if (company == null)
                return NotFound(new { message = "Şirket bulunamadı" });
            
            return Ok(company);
        }

        [HttpPut("{id:int}")]
        [Authorize(Policy = "SuperAdminOnly")]
        public async Task<IActionResult> Update(int id, [FromBody] CompanyDto dto)
        {
            if (id != dto.Id)
                return BadRequest(new { message = "ID uyuşmazlığı" });
            
            var company = await _context.Companies.FirstOrDefaultAsync(c => c.Id == id);
            if (company == null)
                return NotFound(new { message = "Şirket bulunamadı" });
            
            company.Update(dto.Name, dto.Address, dto.PhoneNumber, dto.Email);
            await _context.SaveChangesAsync();
            
            return Ok(new { message = "Şirket bilgileri güncellendi" });
        }

        [HttpPost("{id:int}/approve")]
        [Authorize(Policy = "SuperAdminOnly")]
        public async Task<IActionResult> Approve(int id)
        {
            var company = await _context.Companies.FirstOrDefaultAsync(c => c.Id == id);
            if (company == null) return NotFound(new { message = "Şirket bulunamadı" });
            
            company.Approve();
            await _context.SaveChangesAsync();
            return Ok(new { message = "Şirket başarıyla onaylandı" });
        }

        [HttpPost("{id:int}/reject")]
        [Authorize(Policy = "SuperAdminOnly")]
        public async Task<IActionResult> Reject(int id)
        {
            var company = await _context.Companies.FirstOrDefaultAsync(c => c.Id == id);
            if (company == null) return NotFound(new { message = "Şirket bulunamadı" });
            
            company.Reject();
            await _context.SaveChangesAsync();
            return Ok(new { message = "Şirket onayı reddedildi" });
        }

        [HttpPost("{id:int}/deactivate")]
        [Authorize(Policy = "SuperAdminOnly")]
        public async Task<IActionResult> Deactivate(int id)
        {
            var company = await _context.Companies.FirstOrDefaultAsync(c => c.Id == id);
            if (company == null) return NotFound(new { message = "Şirket bulunamadı" });
            
            company.Deactivate();
            await _context.SaveChangesAsync();
            return Ok(new { message = "Şirket pasifleştirildi" });
        }

        [HttpPost("{id:int}/activate")]
        [Authorize(Policy = "SuperAdminOnly")]
        public async Task<IActionResult> Activate(int id)
        {
            var company = await _context.Companies.FirstOrDefaultAsync(c => c.Id == id);
            if (company == null) return NotFound(new { message = "Şirket bulunamadı" });
            
            company.Activate();
            await _context.SaveChangesAsync();
            return Ok(new { message = "Şirket aktif hale getirildi" });
        }
    }
}
