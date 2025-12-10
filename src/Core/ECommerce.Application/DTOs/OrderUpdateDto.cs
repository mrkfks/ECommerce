using ECommerce.Domain.Enums;

namespace ECommerce.Application.DTOs
{
    public class OrderUpdateDto
    {
        public int Id { get; set; }
        public OrderStatus OrderStatus { get; set; }
        public int? AddressId { get; set; }
    }
}
