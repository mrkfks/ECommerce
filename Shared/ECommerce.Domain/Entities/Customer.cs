namespace ECommerce.Domain.Entities
{
    public class Customer
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Email { get; set; }
        public int UserId { get; set; }
        public required User User { get; set; }
        public required string PhoneNumber { get; set; }
        public required string Address { get; set; }
        public DateTime DateOfBirth { get; set; }

        public required ICollection<Order> Orders { get; set; }
    }
}