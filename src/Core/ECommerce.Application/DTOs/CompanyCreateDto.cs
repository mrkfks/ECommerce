namespace ECommerce.Application.DTOs
{
    public class CompanyCreateDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string TaxNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
    }
}
