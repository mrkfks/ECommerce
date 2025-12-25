namespace ECommerce.Domain.Entities
{
    /// <summary>
    /// Öznitelik değerleri
    /// Örnek: Renk -> Kırmızı, Mavi, Siyah | Beden -> S, M, L, XL
    /// </summary>
    public class AttributeValue : IAuditable
    {
        private AttributeValue() { }

        public int Id { get; private set; }
        public int AttributeId { get; private set; }
        public string Value { get; private set; } = string.Empty;
        public string? ColorCode { get; private set; } // Renk için hex code
        public bool IsActive { get; private set; } = true;
        public int DisplayOrder { get; private set; }
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

        // Navigation Properties
        public virtual ProductAttribute? Attribute { get; private set; }
        public virtual ICollection<ProductVariantAttribute> VariantAttributes { get; private set; } = new List<ProductVariantAttribute>();

        public static AttributeValue Create(int attributeId, string value, string? colorCode = null, int displayOrder = 0)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Değer boş olamaz.", nameof(value));

            if (attributeId <= 0)
                throw new ArgumentException("Geçerli bir öznitelik seçilmelidir.", nameof(attributeId));

            return new AttributeValue
            {
                AttributeId = attributeId,
                Value = value,
                ColorCode = colorCode,
                DisplayOrder = displayOrder,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        public void Update(string value, string? colorCode = null, int displayOrder = 0)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Değer boş olamaz.", nameof(value));

            Value = value;
            ColorCode = colorCode;
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
