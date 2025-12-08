namespace ECommerce.Application.DTOs
{
    public class CustomerDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime DateOfBirth { get; set; }

        public int UserId { get; set; }
        public string UserName { get; set; }
        public List<AddressDto> Address { get; set; } = new();
        public List<OrderDto> Orders { get; set; } = new();
        public List<ReviewDto> Reviews { get; set; } = new();
    }
}