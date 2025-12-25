namespace ECommerce.Domain.Entities
{
    public class Brand : IAuditable
    {
        private Brand() { }

        public int Id { get; private set; }
        public string Name { get; private set; } = string.Empty;
        public string Description { get; private set; } = string.Empty;
        public string? ImageUrl { get; private set; }
        public int CompanyId { get; private set; }
        public int? CategoryId { get; private set; } // Opsiyonel kategori ilişkisi
        public bool IsActive { get; private set; } = true;
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;
        
        // Navigation Properties
        public virtual Category? Category { get; private set; }
        public virtual ICollection<Product> Products { get; private set; } = new List<Product>();
        public virtual ICollection<Model> Models { get; private set; } = new List<Model>();

        public static Brand Create(string name, string description, int companyId, int? categoryId = null, string? imageUrl = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Marka adı boş olamaz.", nameof(name));
            
            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentException("Marka açıklaması boş olamaz.", nameof(description));

            return new Brand
            {
                Name = name,
                Description = description,
                CompanyId = companyId,
                CategoryId = categoryId,
                ImageUrl = imageUrl,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        public void Update(string name, string description, int? categoryId = null, string? imageUrl = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Marka adı boş olamaz.", nameof(name));
            
            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentException("Marka açıklaması boş olamaz.", nameof(description));

            Name = name;
            Description = description;
            CategoryId = categoryId;
            ImageUrl = imageUrl;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetCategory(int? categoryId)
        {
            CategoryId = categoryId;
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