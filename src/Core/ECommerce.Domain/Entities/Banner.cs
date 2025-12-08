namespace ECommerce.Domain.Entities
{
    public class Banner
    {
        public int Id { get; set; }
        public required string Title { get; set; }
        public required string ImageUrl { get; set; }
        public required string RedirectUrl { get; set; }
    }
}