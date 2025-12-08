namespace ECommerce.Application.DTOs
{
    public class OrderCreateDto
    {
        public int CustomerId { get; set; }
        public List<OrderItemCreateDto> Items { get; set; } = new();
    }
}
