namespace ECommerce.Domain.Entities
{
    public class Review : IAuditable, ITenantEntity
    {
        private Review() { }

        public int Id { get; private set; }
        public int ProductId { get; private set; }
        public int CustomerId { get; private set; }
        public int CompanyId { get; private set; }
        public string ReviewerName { get; private set; } = string.Empty;
        public int Rating { get; private set; } // 1 to 5
        public string Comment { get; private set; } = string.Empty;
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;
        
        public virtual Product? Product { get; private set; }
        public virtual Customer? Customer { get; private set; }
        public virtual Company? Company { get; private set; }

        public static Review Create(int productId, int customerId, int companyId, string reviewerName, int rating, string comment)
        {
            if (string.IsNullOrWhiteSpace(reviewerName))
                throw new ArgumentException("Yorum yapan adı boş olamaz.", nameof(reviewerName));
            
            if (rating < 1 || rating > 5)
                throw new ArgumentException("Derecelendirme 1 ile 5 arasında olmalıdır.", nameof(rating));
            
            if (string.IsNullOrWhiteSpace(comment))
                throw new ArgumentException("Yorum metni boş olamaz.", nameof(comment));

            return new Review
            {
                ProductId = productId,
                CustomerId = customerId,
                CompanyId = companyId,
                ReviewerName = reviewerName,
                Rating = rating,
                Comment = comment,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        public void Update(int rating, string comment)
        {
            if (rating < 1 || rating > 5)
                throw new ArgumentException("Derecelendirme 1 ile 5 arasında olmalıdır.", nameof(rating));
            
            if (string.IsNullOrWhiteSpace(comment))
                throw new ArgumentException("Yorum metni boş olamaz.", nameof(comment));

            Rating = rating;
            Comment = comment;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}