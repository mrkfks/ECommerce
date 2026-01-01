namespace ECommerce.Application.DTOs
{
    public class OrderStatusUpdateDto
    {
        public int OrderId { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class QuickOrderInfoDto
    {
        public int Id { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string StatusText { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public DateTime OrderDate { get; set; }
        public int ItemCount { get; set; }
    }
}
