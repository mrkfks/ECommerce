namespace ECommerce.Domain.Entities
{
    /// <summary>
    /// Kategori özelliklerinin değerleri (örn: "Kırmızı", "Mavi" for "Renk")
    /// </summary>
    public class CategoryAttributeValue : IAuditable
    {
        private CategoryAttributeValue() { }

        public int Id { get; private set; }
        public int CategoryAttributeId { get; private set; }
        public string Value { get; private set; } = string.Empty; // Değer (örn: "Kırmızı", "XL", "8GB")
        public string? ColorCode { get; private set; } // Renk kodları için (#FF0000)
        public int DisplayOrder { get; private set; }
        public bool IsActive { get; private set; } = true;
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

        // Navigation Properties
        public virtual CategoryAttribute CategoryAttribute { get; private set; } = null!;

        public static CategoryAttributeValue Create(int categoryAttributeId, string value, string? colorCode = null, int displayOrder = 0)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Özellik değeri boş olamaz.", nameof(value));

            return new CategoryAttributeValue
            {
                CategoryAttributeId = categoryAttributeId,
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
                throw new ArgumentException("Özellik değeri boş olamaz.", nameof(value));

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
