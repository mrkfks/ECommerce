namespace ECommerce.Application.DTOs
{
    public class AddressDto
    {
        public int Id { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string PostalCode { get; set; }

        public int CustomerId { get; set; }
        public string CustomerName { get; set; }

        public int? CompanyId { get; set; }
        public string CompanyName { get; set; }
    }
}
