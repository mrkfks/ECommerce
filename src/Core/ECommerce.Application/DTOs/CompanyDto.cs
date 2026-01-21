namespace ECommerce.Application.DTOs
{
    public class CompanyDto
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Address { get; set; }
        public required string PhoneNumber { get; set; }
        public required string Email { get; set; }
        public required string TaxNumber { get; set; }
        public bool IsActive { get; set; }
        public bool IsApproved { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int UserCount { get; set; }
        public int CustomerCount { get; set; }
        
        // Sorumlu Kişi Bilgileri
        public string? ResponsiblePersonName { get; set; }
        public string? ResponsiblePersonPhone { get; set; }
        public string? ResponsiblePersonEmail { get; set; }

        // Branding
        public string? Domain { get; set; }
        public string? LogoUrl { get; set; }
        public string? PrimaryColor { get; set; }
        public string? SecondaryColor { get; set; }
    }
}