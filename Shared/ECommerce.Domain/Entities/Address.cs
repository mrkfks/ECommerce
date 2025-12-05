namespace ECommerce.Domain.Entities
{
    public class Address
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public required string Street { get; set; }
        public required string City { get; set; }
        public required string State { get; set; }
        public required string ZipCode { get; set; }
        public required string Country { get; set; }
        
        public required virtual Customer Customer { get; set; }
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}