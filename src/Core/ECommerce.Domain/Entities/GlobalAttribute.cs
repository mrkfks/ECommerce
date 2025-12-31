namespace ECommerce.Domain.Entities
{
    /// <summary>
    /// Tüm kategoriler ve ürünler için kullanılabilen global dinamik özellikler
    /// Örneğin: Renk, Boyut, RAM, Depolama vs.
    /// Bir özellik birden fazla kategoride veya ürün türünde kullanılabilir
    /// </summary>
    public class GlobalAttribute : IAuditable
    {
        private GlobalAttribute() { }

        public int Id { get; private set; }
        public string Name { get; private set; } = string.Empty; // Sistem adı (örn: "color", "size")
        public string DisplayName { get; private set; } = string.Empty; // Görüntüleme adı (örn: "Renk", "Boyut")
        public string Description { get; private set; } = string.Empty; // Açıklama
        public int CompanyId { get; private set; }
        
        // Özellik tipi (Text, Color, Select, Range, Boolean)
        public AttributeType AttributeType { get; private set; } = AttributeType.Text;
        
        public int DisplayOrder { get; private set; }
        public bool IsActive { get; private set; } = true;
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

        // Navigation Properties
        public virtual ICollection<GlobalAttributeValue> Values { get; private set; } = new List<GlobalAttributeValue>();
        public virtual ICollection<CategoryGlobalAttribute> CategoryMappings { get; private set; } = new List<CategoryGlobalAttribute>();

        public static GlobalAttribute Create(
            string name, 
            string displayName, 
            string description,
            int companyId,
            AttributeType attributeType = AttributeType.Text,
            int displayOrder = 0)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Özellik adı boş olamaz.", nameof(name));

            if (string.IsNullOrWhiteSpace(displayName))
                throw new ArgumentException("Görüntüleme adı boş olamaz.", nameof(displayName));

            return new GlobalAttribute
            {
                Name = name,
                DisplayName = displayName,
                Description = description ?? string.Empty,
                CompanyId = companyId,
                AttributeType = attributeType,
                DisplayOrder = displayOrder,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        public void Update(string displayName, string description, AttributeType attributeType, int displayOrder = 0)
        {
            if (string.IsNullOrWhiteSpace(displayName))
                throw new ArgumentException("Görüntüleme adı boş olamaz.", nameof(displayName));

            DisplayName = displayName;
            Description = description ?? string.Empty;
            AttributeType = attributeType;
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

    /// <summary>
    /// Global özellik değerleri (örn: Renk özelliği için "Kırmızı", "Mavi", "Yeşil")
    /// </summary>
    public class GlobalAttributeValue : IAuditable
    {
        private GlobalAttributeValue() { }

        public int Id { get; private set; }
        public int GlobalAttributeId { get; private set; }
        public string Value { get; private set; } = string.Empty; // Değer (örn: "Kırmızı", "XL", "8GB")
        public string? ColorCode { get; private set; } // Renk kodları için (#FF0000)
        public int DisplayOrder { get; private set; }
        public bool IsActive { get; private set; } = true;
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

        // Navigation Properties
        public virtual GlobalAttribute GlobalAttribute { get; private set; } = null!;

        public static GlobalAttributeValue Create(int globalAttributeId, string value, string? colorCode = null, int displayOrder = 0)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Değer boş olamaz.", nameof(value));

            return new GlobalAttributeValue
            {
                GlobalAttributeId = globalAttributeId,
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

    /// <summary>
    /// Kategori - Global Attribute many-to-many ilişkisi
    /// Kategori hangi global özellikleri kullanacağını belirler
    /// </summary>
    public class CategoryGlobalAttribute : IAuditable
    {
        private CategoryGlobalAttribute() { }

        public int Id { get; private set; }
        public int CategoryId { get; private set; }
        public int GlobalAttributeId { get; private set; }
        public bool IsRequired { get; private set; } = false; // Bu kategori için ürün eklerken zorunlu mu?
        public int DisplayOrder { get; private set; }
        public bool IsActive { get; private set; } = true;
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

        // Navigation Properties
        public virtual Category Category { get; private set; } = null!;
        public virtual GlobalAttribute GlobalAttribute { get; private set; } = null!;

        public static CategoryGlobalAttribute Create(int categoryId, int globalAttributeId, bool isRequired = false, int displayOrder = 0)
        {
            return new CategoryGlobalAttribute
            {
                CategoryId = categoryId,
                GlobalAttributeId = globalAttributeId,
                IsRequired = isRequired,
                DisplayOrder = displayOrder,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        public void Update(bool isRequired, int displayOrder)
        {
            IsRequired = isRequired;
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

    /// <summary>
    /// Brand - Category many-to-many ilişkisi
    /// Bir marka birden fazla kategoride ürün üretebilir
    /// </summary>
    public class BrandCategory : IAuditable
    {
        private BrandCategory() { }

        public int Id { get; private set; }
        public int BrandId { get; private set; }
        public int CategoryId { get; private set; }
        public bool IsActive { get; private set; } = true;
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

        // Navigation Properties
        public virtual Brand Brand { get; private set; } = null!;
        public virtual Category Category { get; private set; } = null!;

        public static BrandCategory Create(int brandId, int categoryId)
        {
            return new BrandCategory
            {
                BrandId = brandId,
                CategoryId = categoryId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
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

    /// <summary>
    /// Özellik türleri
    /// </summary>
    public enum AttributeType
    {
        Text = 0,           // Metin (örn: Model ismi)
        Select = 1,         // Seçim (renk, boyut vb.)
        MultiSelect = 2,    // Çoklu seçim
        Color = 3,          // Renk
        Range = 4,          // Aralık (örn: 0-100)
        Boolean = 5,        // Evet/Hayır
        Date = 6,           // Tarih
        Number = 7          // Sayı
    }
}
