namespace ECommerce.Application.DTOs
{
    public class ReviewCreateDto
    {
        public int ProductId { get; set; }
        public int CustomerId { get; set; }
        public int CompanyId { get; set; }
        public string? ReviewerName { get; set; }
        public int Rating { get; set; }   // 1-5 arası puan
        public string Comment { get; set; } = string.Empty;
    }
}
