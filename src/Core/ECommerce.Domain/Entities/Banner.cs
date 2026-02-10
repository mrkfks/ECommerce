namespace ECommerce.Domain.Entities
{
    public class Banner : BaseEntity, ITenantEntity, ISoftDeletable
    {
        private Banner() { }

        public string Title { get; private set; } = string.Empty;
        public string? Description { get; private set; }
        public string ImageUrl { get; private set; } = string.Empty;
        public string? Link { get; private set; }
        public int Order { get; private set; }
        public bool IsActive { get; private set; } = true;
        public int CompanyId { get; private set; }

        public virtual Company? Company { get; private set; }

        public static Banner Create(string title, string imageUrl, int companyId, string? description = null, string? link = null, int order = 0)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Banner başlığı boş olamaz.", nameof(title));

            if (string.IsNullOrWhiteSpace(imageUrl))
                throw new ArgumentException("Banner resim URL'si boş olamaz.", nameof(imageUrl));

            return new Banner
            {
                Title = title,
                Description = description,
                ImageUrl = imageUrl,
                Link = link,
                Order = order,
                CompanyId = companyId,
                IsActive = true
            };
        }

        public void Update(string title, string imageUrl, string? description = null, string? link = null, int order = 0)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Banner başlığı boş olamaz.", nameof(title));

            if (string.IsNullOrWhiteSpace(imageUrl))
                throw new ArgumentException("Banner resim URL'si boş olamaz.", nameof(imageUrl));

            Title = title;
            Description = description;
            ImageUrl = imageUrl;
            Link = link;
            Order = order;
        }

        public void Activate() => IsActive = true;
        public void Deactivate() => IsActive = false;
        public void SetOrder(int order) => Order = order;
    }
}