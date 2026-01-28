namespace Dashboard.Web.Models
{
    public class CustomerDto
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int TotalOrders { get; set; }
        public decimal TotalSpent { get; set; }
        public List<AddressDto> Addresses { get; set; } = new();
    }

    public class AddressDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string FullAddress { get; set; } = string.Empty;
        public string? City { get; set; }
        public string? District { get; set; }
        public string? PostalCode { get; set; }
        public bool IsDefault { get; set; }
    }
}
