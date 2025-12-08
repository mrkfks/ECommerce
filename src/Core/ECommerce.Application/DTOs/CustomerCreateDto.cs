namespace ECommerce.Application.DTOs
{
    public class CustomerCreateDto
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }

        public int? UserId { get; set; }
    }
}
