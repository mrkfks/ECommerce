namespace ECommerce.Application.DTOs;

/// <summary>
/// Şirket bilgisi DTO
/// </summary>
public record CompanyDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string TaxNumber { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public string Address { get; init; } = string.Empty;
    public bool IsActive { get; init; } = true;
    public bool IsApproved { get; init; } = false;
    public DateTime? CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
    
    // Sorumlu kişi bilgileri
    public string? ResponsiblePersonName { get; init; }
    public string? ResponsiblePersonPhone { get; init; }
    public string? ResponsiblePersonEmail { get; init; }
    
    // İstatistikler
    public int UserCount { get; init; }
    public int CustomerCount { get; init; }
    
    // Branding bilgileri
    public string? Domain { get; init; }
    public string? LogoUrl { get; init; }
    public string? PrimaryColor { get; init; }
    public string? SecondaryColor { get; init; }
}

/// <summary>
/// Şirket oluşturma/güncelleme form DTO
/// </summary>
public record CompanyFormDto
{
    public int? Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string TaxNumber { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public string Address { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool IsActive { get; init; } = true;
    
    // Sorumlu kişi bilgileri
    public string? ResponsiblePersonName { get; init; }
    public string? ResponsiblePersonPhone { get; init; }
    public string? ResponsiblePersonEmail { get; init; }
}

/// <summary>
/// Şirket branding (marka) bilgileri güncelleme DTO
/// </summary>
public record BrandingUpdateDto
{
    public string? Domain { get; init; }
    public string? LogoUrl { get; init; }
    public string? PrimaryColor { get; init; }
    public string? SecondaryColor { get; init; }
}
