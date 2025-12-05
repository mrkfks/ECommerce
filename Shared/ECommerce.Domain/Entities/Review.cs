namespace ECommerce.Domain.Entities
{
    public class Review
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int? CustomerId { get; set; }
        public required string ReviewerName { get; set; }
        public int Rating { get; set; } // e.g., 1 to 5
        public required string Comment { get; set; }
        public virtual required Product Product { get; set; }
        public virtual required Customer Customer { get; set; }
    }
}