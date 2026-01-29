namespace Dashboard.Web.Models;

public class CompanyVm
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string TaxNumber { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool IsApproved { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Sorumlu Kişi Bilgileri
    public string? ResponsiblePersonName { get; set; }
    public string? ResponsiblePersonPhone { get; set; }
    public string? ResponsiblePersonEmail { get; set; }
    
    // Branding Bilgileri
    public string? Domain { get; set; }
    public string? LogoUrl { get; set; }
    public string? PrimaryColor { get; set; }
    public string? SecondaryColor { get; set; }
}

