namespace ECommerce.Domain.Entities
{
    /// <summary>
    /// Model entity - Her model bir markaya bağlıdır
    /// Örnek: Samsung Galaxy S23, iPhone 15 Pro
    /// </summary>
    public class Model : IAuditable
    {
        private Model() { }

        public int Id { get; private set; }
        public string Name { get; private set; } = string.Empty;
        public string Description { get; private set; } = string.Empty;
        public int BrandId { get; private set; }
        public bool IsActive { get; private set; } = true;
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

        // Navigation Properties
        public virtual Brand? Brand { get; private set; }
        public virtual ICollection<Product> Products { get; private set; } = new List<Product>();

        public static Model Create(string name, string description, int brandId)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Model adı boş olamaz.", nameof(name));

            if (brandId <= 0)
                throw new ArgumentException("Geçerli bir marka seçilmelidir.", nameof(brandId));

            return new Model
            {
                Name = name,
                Description = description ?? string.Empty,
                BrandId = brandId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        public void Update(string name, string description)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Model adı boş olamaz.", nameof(name));

            Name = name;
            Description = description ?? string.Empty;
            UpdatedAt = DateTime.UtcNow;
        }

        public void ChangeBrand(int brandId)
        {
            if (brandId <= 0)
                throw new ArgumentException("Geçerli bir marka seçilmelidir.", nameof(brandId));

            BrandId = brandId;
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
