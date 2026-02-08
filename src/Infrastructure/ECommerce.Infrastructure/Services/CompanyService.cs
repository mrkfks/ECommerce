using ECommerce.Application.DTOs;
using ECommerce.Application.Exceptions;
using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ECommerce.Infrastructure.Services;

public class CompanyService : ICompanyService
{
    private readonly AppDbContext _context;
    private readonly ILogger<CompanyService> _logger;

    public CompanyService(AppDbContext context, ILogger<CompanyService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<CompanyRegistrationResultDto> RegisterCompanyAsync(RegisterDto dto)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Email ve Username kontrolleri
            var existingCompany = await _context.Companies
                .FirstOrDefaultAsync(c => c.Email == dto.CompanyEmail);
            
            if (existingCompany != null)
            {
                throw new BusinessException("Bu email adresi ile kayıtlı bir şirket zaten mevcut.");
            }

            var existingUser = await _context.Users
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(u => u.Email == dto.Email || u.Username == dto.Username);
            
            if (existingUser != null)
            {
                if (existingUser.Email == dto.Email)
                     throw new BusinessException("Bu email adresi ile kayıtlı bir kullanıcı zaten mevcut.");
                else
                     throw new BusinessException("Bu kullanıcı adı zaten kullanılıyor.");
            }

            // Yeni şirket oluştur (IsApproved = false)
            var company = Company.Create(
                dto.CompanyName,
                dto.CompanyAddress,
                dto.CompanyPhoneNumber,
                dto.CompanyEmail,
                dto.TaxNumber,
                $"{dto.FirstName} {dto.LastName}", 
                dto.CompanyPhoneNumber,
                dto.Email 
            );

            _context.Companies.Add(company);
            await _context.SaveChangesAsync();

            // Admin kullanıcı oluştur
            // Note: BCrypt dependency needed. Assuming it is available in Infrastructure.
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            
            var adminUser = User.Create(
                company.Id,
                dto.Username,
                dto.Email,
                passwordHash,
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

            return new CompanyRegistrationResultDto
            {
                CompanyId = company.Id,
                CompanyName = company.Name,
                Username = adminUser.Username,
                Message = "Şirket ve kullanıcı kaydınız alınmıştır. Süper admin onayından sonra giriş yapabileceksiniz."
            };
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<CompanyDto> CreateAsync(CompanyFormDto dto)
    {
        // Email kontrolü
        var existingCompany = await _context.Companies.FirstOrDefaultAsync(c => c.Email == dto.Email);
        if (existingCompany != null)
            throw new BusinessException("Bu email adresi ile kayıtlı bir şirket zaten mevcut.");

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

        return new CompanyDto
        {
            Id = company.Id,
            Name = company.Name,
            Address = company.Address,
            PhoneNumber = company.PhoneNumber,
            Email = company.Email,
            TaxNumber = company.TaxNumber,
            ResponsiblePersonName = company.ResponsiblePersonName,
            ResponsiblePersonPhone = company.ResponsiblePersonPhone,
            ResponsiblePersonEmail = company.ResponsiblePersonEmail,
            IsActive = company.IsActive,
            IsApproved = company.IsApproved,
            CreatedAt = company.CreatedAt,
            UpdatedAt = company.UpdatedAt,
            UserCount = 0,
            CustomerCount = 0
        };
    }

    public async Task<IReadOnlyList<CompanyDto>> GetAllAsync()
    {
        return await _context.Companies
            .IgnoreQueryFilters() // SuperAdmin tüm şirketleri görebilmeli
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
    }

    public async Task<CompanyDto?> GetByIdAsync(int id)
    {
         return await _context.Companies
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
    }

    public async Task UpdateAsync(int id, CompanyFormDto dto)
    {
        var company = await _context.Companies.FirstOrDefaultAsync(c => c.Id == id);
        if (company == null) throw new KeyNotFoundException("Şirket bulunamadı");
        
        company.Update(dto.Name, dto.Address, dto.PhoneNumber, dto.Email);
        await _context.SaveChangesAsync();
    }

    public async Task ApproveAsync(int id)
    {
        var company = await _context.Companies.FirstOrDefaultAsync(c => c.Id == id);
        if (company == null) throw new KeyNotFoundException("Şirket bulunamadı");
        
        company.Approve();
        await _context.SaveChangesAsync();
    }

    public async Task RejectAsync(int id)
    {
        var company = await _context.Companies.FirstOrDefaultAsync(c => c.Id == id);
        if (company == null) throw new KeyNotFoundException("Şirket bulunamadı");
        
        company.Reject();
        await _context.SaveChangesAsync();
    }

    public async Task ActivateAsync(int id)
    {
        var company = await _context.Companies.FirstOrDefaultAsync(c => c.Id == id);
        if (company == null) throw new KeyNotFoundException("Şirket bulunamadı");
        
        company.Activate();
        await _context.SaveChangesAsync();
    }

    public async Task DeactivateAsync(int id)
    {
        var company = await _context.Companies.FirstOrDefaultAsync(c => c.Id == id);
        if (company == null) throw new KeyNotFoundException("Şirket bulunamadı");
        
        company.Deactivate();
        await _context.SaveChangesAsync();
    }

    public async Task<CompanyDto?> GetByDomainAsync(string domain)
    {
        var normalizedDomain = domain.Trim().ToLower();
        var company = await _context.Companies
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Domain == normalizedDomain);

        if (company == null) return null;

        return new CompanyDto
        {
            Id = company.Id,
            Name = company.Name,
            Address = company.Address,
            PhoneNumber = company.PhoneNumber,
            Email = company.Email,
            TaxNumber = company.TaxNumber,
            ResponsiblePersonName = company.ResponsiblePersonName,
            ResponsiblePersonPhone = company.ResponsiblePersonPhone,
            ResponsiblePersonEmail = company.ResponsiblePersonEmail,
            IsActive = company.IsActive,
            IsApproved = company.IsApproved,
            CreatedAt = company.CreatedAt,
            UpdatedAt = company.UpdatedAt,
            // Branding fields
            Domain = company.Domain,
            LogoUrl = company.LogoUrl,
            PrimaryColor = company.PrimaryColor,
            SecondaryColor = company.SecondaryColor
        };
    }

    public async Task UpdateBrandingAsync(int id, BrandingUpdateDto dto)
    {
        var company = await _context.Companies.FirstOrDefaultAsync(c => c.Id == id);
        if (company == null) throw new KeyNotFoundException("Şirket bulunamadı");
        
        company.UpdateBranding(dto.Domain, dto.LogoUrl, dto.PrimaryColor, dto.SecondaryColor);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateLogoAsync(int id, string logoUrl)
    {
        var company = await _context.Companies.FirstOrDefaultAsync(c => c.Id == id);
        if (company == null) throw new KeyNotFoundException("Şirket bulunamadı");
        
        company.UpdateBranding(company.Domain, logoUrl, company.PrimaryColor, company.SecondaryColor);
        await _context.SaveChangesAsync();
    }

    public async Task<object> GetBrandingByDomainAsync(string domain)
    {
        if (string.IsNullOrWhiteSpace(domain))
            throw new ArgumentException("Domain boş olamaz", nameof(domain));

        var company = await _context.Companies
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Domain != null && c.Domain.ToLower() == domain.ToLower());

        if (company == null)
        {
            // Fallback: localhost ya da default domain durumunda ilk aktif şirketi döndür
            if (domain == "localhost" || domain.StartsWith("localhost:"))
            {
                company = await _context.Companies
                    .AsNoTracking()
                    .Where(c => c.IsActive && c.IsApproved)
                    .FirstOrDefaultAsync();
                
                if (company == null)
                {
                    throw new KeyNotFoundException($"Domain '{domain}' için şirket bulunamadı ve fallback şirketi de yok");
                }
            }
            else
            {
                throw new KeyNotFoundException($"Domain '{domain}' için şirket bulunamadı");
            }
        }

        return new
        {
            companyId = company.Id,
            companyName = company.Name,
            logoUrl = company.LogoUrl,
            primaryColor = company.PrimaryColor,
            secondaryColor = company.SecondaryColor,
            domain = company.Domain
        };
    }
}

