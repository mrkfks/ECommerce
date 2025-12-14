namespace ECommerce.Application.DTOs
{
    public class RegisterDto
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
        public int CompanyId { get; set; } // Will be ignored if creating new company
        
        // Company Registration Fields
        public string CompanyName { get; set; } = string.Empty;
        public string CompanyAddress { get; set; } = string.Empty;
        public string CompanyPhoneNumber { get; set; } = string.Empty;
        public string CompanyEmail { get; set; } = string.Empty;
        public string TaxNumber { get; set; } = string.Empty;
    }
}
