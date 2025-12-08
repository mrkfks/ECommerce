namespace ECommerce.Application.DTOs
{
    public class OrderUpdateDto
    {
        public int Id { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
