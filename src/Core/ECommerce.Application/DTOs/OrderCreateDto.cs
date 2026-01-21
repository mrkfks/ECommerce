namespace ECommerce.Application.DTOs
{
    public class OrderCreateDto
    {
        public int CustomerId { get; set; }
        public int AddressId { get; set; }
        public int CompanyId { get; set; }
        public List<OrderItemCreateDto> Items { get; set; } = new();
        public AddressCreateDto? ShippingAddress { get; set; }
        
        // Ödeme Bilgileri
        public string? CardNumber { get; set; }
        public string? CardExpiry { get; set; }
        public string? CardCvv { get; set; }
    }
}
