namespace ECommerce.Domain.Entities
{
    public class Product : IAuditable, ITenantEntity
    {
        private Product() { }

        public int Id { get; private set; }
        public string Name { get; private set; } = string.Empty;
        public string Description { get; private set; } = string.Empty;
        public decimal Price { get; private set; }
        public int CategoryId { get; private set; }
        public int BrandId { get; private set; }
        public int CompanyId { get; private set; }
        public int StockQuantity { get; private set; }
        public string? ImageUrl { get; private set; }
        public bool IsActive { get; private set; } = true;
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

        public virtual Category? Category { get; private set; }
        public virtual Brand? Brand { get; private set; }
        public virtual Company? Company { get; private set; }
        public virtual ICollection<Review> Reviews { get; private set; } = new List<Review>();
        public virtual ICollection<OrderItem> OrderItems { get; private set; } = new List<OrderItem>();
        
        [System.ComponentModel.DataAnnotations.ConcurrencyCheck]
        public Guid Version { get; private set; } = Guid.NewGuid();

        public static Product Create(string name, string description, decimal price, int categoryId, int brandId, int companyId, int stockQuantity, string? imageUrl = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Ürün adı boş olamaz.", nameof(name));
            
            if (price <= 0)
                throw new ArgumentException("Ürün fiyatı sıfırdan büyük olmalıdır.", nameof(price));
            
            if (stockQuantity < 0)
                throw new ArgumentException("Stok miktarı negatif olamaz.", nameof(stockQuantity));

            return new Product
            {
                Name = name,
                Description = description,
                Price = price,
                CategoryId = categoryId,
                BrandId = brandId,
                CompanyId = companyId,
                StockQuantity = stockQuantity,
                ImageUrl = imageUrl,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Version = Guid.NewGuid()
            };
        }

        public void Update(string name, string description, decimal price, string? imageUrl = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Ürün adı boş olamaz.", nameof(name));
            
            if (price <= 0)
                throw new ArgumentException("Ürün fiyatı sıfırdan büyük olmalıdır.", nameof(price));

            Name = name;
            Description = description;
            Price = price;
            ImageUrl = imageUrl;
            UpdatedAt = DateTime.UtcNow;
            Version = Guid.NewGuid();
        }

        public void UpdateStock(int quantity)
        {
            if (quantity < 0)
                throw new ArgumentException("Stok miktarı negatif olamaz.", nameof(quantity));
            
            StockQuantity = quantity;
            UpdatedAt = DateTime.UtcNow;
            Version = Guid.NewGuid();
        }

        public void DecreaseStock(int quantity)
        {
            if (quantity <= 0)
                throw new ArgumentException("Azaltılacak miktar sıfırdan büyük olmalıdır.", nameof(quantity));
            
            if (StockQuantity < quantity)
                throw new InvalidOperationException($"Yeterli stok yok. Mevcut: {StockQuantity}, İstenen: {quantity}");
            
            StockQuantity -= quantity;
            UpdatedAt = DateTime.UtcNow;
            Version = Guid.NewGuid();
        }

        public void Activate()
        {
            IsActive = true;
            UpdatedAt = DateTime.UtcNow;
            Version = Guid.NewGuid();
        }

        public void Deactivate()
        {
            IsActive = false;
            UpdatedAt = DateTime.UtcNow;
            Version = Guid.NewGuid();
        }
    }
}