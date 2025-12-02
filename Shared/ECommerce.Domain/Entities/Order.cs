using ECommerce.Domain.Enums;

namespace ECommerce.Domain.Entities
{
    public class Order
    {
        public int Id { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public required OrderStatus Status { get; set; } = OrderStatus.Pending;
        public int CustomerId { get; set; }
        public required Customer Customer { get; set; }
        public required ICollection<OrderItem> OrderItems { get; set; }
    }
}