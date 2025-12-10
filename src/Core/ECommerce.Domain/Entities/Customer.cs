namespace ECommerce.Domain.Entities
{
    public class Customer
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string Email { get; set; }
        public int? UserId { get; set; }
        public required string PhoneNumber { get; set; }
        public DateTime DateOfBirth { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public virtual User? User { get; set; }
        public virtual Company? Company { get; set; }
        public virtual ICollection<Address>? Addresses { get; set; } = new List<Address>();
        public virtual ICollection<Review>? Reviews { get; set; } = new List<Review>();
        public virtual ICollection<Order>? Orders { get; set; } = new List<Order>();
    }
}