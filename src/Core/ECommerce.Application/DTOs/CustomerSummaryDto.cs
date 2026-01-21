namespace ECommerce.Application.DTOs
{
    public class CustomerSummaryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public int TotalOrders { get; set; }
        public decimal TotalSpent { get; set; }
        public int OrderCount { get; set; }
        public int ReviewCount { get; set; }
    }
}
