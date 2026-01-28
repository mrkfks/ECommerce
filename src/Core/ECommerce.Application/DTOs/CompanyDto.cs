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
/// Şirket oluşturma DTO
/// </summary>
public record CompanyCreateDto
{
    public string Name { get; init; } = string.Empty;
    public string TaxNumber { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public string Address { get; init; } = string.Empty;
    public string? Description { get; init; }
}

/// <summary>
/// Şirket güncelleme DTO
/// </summary>
public record CompanyUpdateDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public string Address { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public string? Description { get; init; }
}

/// <summary>
/// Yorum bilgisi DTO
/// </summary>
public record ReviewDto
{
    public int Id { get; init; }
    public int ProductId { get; init; }
    public int CustomerId { get; init; }
    public int CompanyId { get; init; }
    public int Rating { get; init; }
    public string Comment { get; init; } = string.Empty;
    public string? ProductName { get; init; }
    public string? CustomerName { get; init; }
    public string? ReviewerName { get; init; }
    public DateTime? CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}
