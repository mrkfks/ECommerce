namespace ECommerce.Domain.Entities
{
    /// <summary>
    /// Kategori entity - Hiyerarşik yapı ile alt kategori desteği
    /// </summary>
    public class Category : IAuditable
    {
        private Category() { }

        public int Id { get; private set; }
        public string Name { get; private set; } = string.Empty;
        public string Description { get; private set; } = string.Empty;
        public string? ImageUrl { get; private set; }
        public int CompanyId { get; private set; }
        public int? ParentCategoryId { get; private set; } // Hiyerarşik yapı için
        public int DisplayOrder { get; private set; }
        public bool IsActive { get; private set; } = true;
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;
        
        // Navigation Properties
        public virtual Category? ParentCategory { get; private set; }
        public virtual ICollection<Category> SubCategories { get; private set; } = new List<Category>();
        public virtual ICollection<Product> Products { get; private set; } = new List<Product>();
        public virtual ICollection<Brand> Brands { get; private set; } = new List<Brand>();
        public virtual ICollection<CategoryAttribute> Attributes { get; private set; } = new List<CategoryAttribute>();

        public static Category Create(string name, string description, string? imageUrl, int companyId, int? parentCategoryId = null, int displayOrder = 0)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Kategori adı boş olamaz.", nameof(name));

            return new Category
            {
                Name = name,
                Description = description ?? string.Empty,
                ImageUrl = imageUrl,
                CompanyId = companyId,
                ParentCategoryId = parentCategoryId,
                DisplayOrder = displayOrder,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        public void Update(string name, string description, string? imageUrl, int? parentCategoryId = null, int displayOrder = 0)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Kategori adı boş olamaz.", nameof(name));

            // Kendi alt kategorisi olamaz kontrolü
            if (parentCategoryId == Id)
                throw new InvalidOperationException("Bir kategori kendi alt kategorisi olamaz.");

            Name = name;
            Description = description ?? string.Empty;
            ImageUrl = imageUrl;
            ParentCategoryId = parentCategoryId;
            DisplayOrder = displayOrder;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetParentCategory(int? parentCategoryId)
        {
            if (parentCategoryId == Id)
                throw new InvalidOperationException("Bir kategori kendi alt kategorisi olamaz.");

            ParentCategoryId = parentCategoryId;
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