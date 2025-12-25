namespace ECommerce.Domain.Entities
{
    /// <summary>
    /// Ürün özellikleri - Dinamik key-value yapısı
    /// Örnek: Ekran Boyutu: 6.7", RAM: 8GB, İşlemci: Snapdragon 8 Gen 2
    /// </summary>
    public class ProductSpecification : IAuditable, ITenantEntity
    {
        private ProductSpecification() { }

        public int Id { get; private set; }
        public int ProductId { get; private set; }
        public int CompanyId { get; private set; }
        public string Key { get; private set; } = string.Empty;
        public string Value { get; private set; } = string.Empty;
        public int DisplayOrder { get; private set; }
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

        // Navigation Properties
        public virtual Product? Product { get; private set; }

        public static ProductSpecification Create(int productId, int companyId, string key, string value, int displayOrder = 0)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Özellik adı boş olamaz.", nameof(key));

            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Özellik değeri boş olamaz.", nameof(value));

            return new ProductSpecification
            {
                ProductId = productId,
                CompanyId = companyId,
                Key = key,
                Value = value,
                DisplayOrder = displayOrder,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        public void Update(string key, string value, int displayOrder = 0)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Özellik adı boş olamaz.", nameof(key));

            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Özellik değeri boş olamaz.", nameof(value));

            Key = key;
            Value = value;
            DisplayOrder = displayOrder;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
