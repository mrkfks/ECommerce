namespace ECommerce.Domain.Entities
{
    /// <summary>
    /// Kategoriye özgü dinamik özellikler (örn: Renk, Boyut, RAM)
    /// </summary>
    public class CategoryAttribute : IAuditable
    {
        private CategoryAttribute() { }

        public int Id { get; private set; }
        public int CategoryId { get; private set; }
        public string Name { get; private set; } = string.Empty; // Özellik adı (örn: "Renk", "Boyut")
        public string DisplayName { get; private set; } = string.Empty; // Gösterim adı
        public int DisplayOrder { get; private set; }
        public bool IsRequired { get; private set; } // Ürün eklerken zorunlu mu?
        public bool IsActive { get; private set; } = true;
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

        // Navigation Properties
        public virtual Category? Category { get; private set; }
        public virtual ICollection<CategoryAttributeValue> Values { get; private set; } = new List<CategoryAttributeValue>();

        public static CategoryAttribute Create(int categoryId, string name, string displayName, int displayOrder = 0, bool isRequired = false)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Özellik adı boş olamaz.", nameof(name));

            return new CategoryAttribute
            {
                CategoryId = categoryId,
                Name = name,
                DisplayName = displayName ?? name,
                DisplayOrder = displayOrder,
                IsRequired = isRequired,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        public void Update(string name, string displayName, int displayOrder = 0, bool isRequired = false)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Özellik adı boş olamaz.", nameof(name));

            Name = name;
            DisplayName = displayName ?? name;
            DisplayOrder = displayOrder;
            IsRequired = isRequired;
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
