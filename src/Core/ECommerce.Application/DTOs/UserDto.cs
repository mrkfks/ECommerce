namespace ECommerce.Application.DTOs
{
    public class UserDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new();
        public int? CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public int CompanyId { get; set; }
        public string? CompanyName { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

