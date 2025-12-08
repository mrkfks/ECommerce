namespace ECommerce.Application.DTOs
{
    public class ReviewUpdateDto
    {
        public int Id { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
    }
}
