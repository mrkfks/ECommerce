namespace ECommerce.Domain.Entities
{
    /// <summary>
    /// Ürün özellikleri - Dinamik key-value yapısı
    /// Örnek: Ekran Boyutu: 6.7", RAM: 8GB, İşlemci: Snapdragon 8 Gen 2
    /// </summary>
    public class ProductSpecification : BaseEntity, ITenantEntity
    {
        private ProductSpecification() { }

        public int ProductId { get; private set; }
        public int CompanyId { get; private set; }
        public string Key { get; private set; } = string.Empty;
        public string Value { get; private set; } = string.Empty;
        public int DisplayOrder { get; private set; }

        // Navigation Properties
        public virtual Product Product { get; private set; } = null!;

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
                DisplayOrder = displayOrder
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
