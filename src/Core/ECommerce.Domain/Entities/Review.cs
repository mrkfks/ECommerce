namespace ECommerce.Domain.Entities
{
    public class Review
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int CustomerId { get; set; }
        public int CompanyId { get; set; }
        public required string ReviewerName { get; set; }
        public int Rating { get; set; } // e.g., 1 to 5
        public required string Comment { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        public virtual Product? Product { get; set; }
        public virtual Customer? Customer { get; set; }
        public virtual Company? Company { get; set; }
    }
}