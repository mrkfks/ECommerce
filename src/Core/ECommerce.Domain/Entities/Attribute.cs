namespace ECommerce.Domain.Entities
{
    /// <summary>
    /// Ürün varyant öznitelikleri
    /// Örnek: Renk, Beden, Bellek Kapasitesi
    /// </summary>
    public class ProductAttribute : IAuditable
    {
        private ProductAttribute() { }

        public int Id { get; private set; }
        public string Name { get; private set; } = string.Empty;
        public string DisplayName { get; private set; } = string.Empty;
        public bool IsActive { get; private set; } = true;
        public int DisplayOrder { get; private set; }
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

        // Navigation Properties
        public virtual ICollection<AttributeValue> Values { get; private set; } = new List<AttributeValue>();

        public static ProductAttribute Create(string name, string displayName, int displayOrder = 0)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Öznitelik adı boş olamaz.", nameof(name));

            return new ProductAttribute
            {
                Name = name,
                DisplayName = displayName ?? name,
                DisplayOrder = displayOrder,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        public void Update(string name, string displayName, int displayOrder = 0)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Öznitelik adı boş olamaz.", nameof(name));

            Name = name;
            DisplayName = displayName ?? name;
            DisplayOrder = displayOrder;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Activate()
        {
            IsActive = true;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Deactivate()
        {
            IsActive = false;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
