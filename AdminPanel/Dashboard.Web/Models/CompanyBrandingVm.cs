namespace Dashboard.Web.Models;

/// <summary>
/// Şirket marka ayarları (branding) view model
/// </summary>
public class CompanyBrandingVm
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Domain { get; set; }
    public string? LogoUrl { get; set; }
    public string PrimaryColor { get; set; } = "#3b82f6";
    public string SecondaryColor { get; set; } = "#1e40af";
}
