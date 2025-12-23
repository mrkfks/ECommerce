namespace ECommerce.Domain.Entities
{
    public class Brand : IAuditable
    {
        private Brand() { }

        public int Id { get; private set; }
        public string Name { get; private set; } = string.Empty;
        public string Description { get; private set; } = string.Empty;
        public string? ImageUrl { get; private set; }
        public bool IsActive { get; private set; } = true;
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;
        
        public virtual ICollection<Product> Products { get; private set; } = new List<Product>();

        public static Brand Create(string name, string description, string? imageUrl = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Marka adı boş olamaz.", nameof(name));
            
            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentException("Marka açıklaması boş olamaz.", nameof(description));

            return new Brand
            {
                Name = name,
                Description = description,
                ImageUrl = imageUrl,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        public void Update(string name, string description, string? imageUrl = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Marka adı boş olamaz.", nameof(name));
            
            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentException("Marka açıklaması boş olamaz.", nameof(description));

            Name = name;
            Description = description;
            ImageUrl = imageUrl;
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