namespace ECommerce.Application.DTOs
{
    public class CustomerCreateDto
    {
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string Email { get; set; }
        public required string PhoneNumber { get; set; }
        public DateTime DateOfBirth { get; set; }
        public int CompanyId { get; set; }
        public int? UserId { get; set; }
    }
}
