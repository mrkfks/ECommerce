namespace ECommerce.Application.DTOs
{
    public class OrderDto
    {
        public int Id { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public CustomerDto Customer { get; set; } = new CustomerDto();
        public List<OrderItemDto> Items { get; set; } = new();
    }
}
