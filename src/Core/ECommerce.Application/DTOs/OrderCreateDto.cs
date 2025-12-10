namespace ECommerce.Application.DTOs
{
    public class OrderCreateDto
    {
        public int CustomerId { get; set; }
        public int AddressId { get; set; }
        public int CompanyId { get; set; }
        public List<OrderItemCreateDto> Items { get; set; } = new();
    }
}
