namespace ECommerce.Domain.Entities
{
    public class Campaign : BaseEntity, ITenantEntity
    {
        private Campaign() { }

        public string Name { get; private set; } = string.Empty;
        public string? Description { get; private set; }
        public string? BannerImageUrl { get; private set; }
        public decimal DiscountPercent { get; private set; }
        public DateTime StartDate { get; private set; }
        public DateTime EndDate { get; private set; }
        public bool IsActive { get; private set; }
        public int CompanyId { get; private set; }

        // İlişkiler
        public virtual Company? Company { get; private set; }
        public virtual ICollection<ProductCampaign> Products { get; private set; } = new List<ProductCampaign>();
        public virtual ICollection<CategoryCampaign> Categories { get; private set; } = new List<CategoryCampaign>();

        // Hesaplama özellikleri
        public bool IsCurrentlyActive => IsActive && DateTime.UtcNow >= StartDate && DateTime.UtcNow <= EndDate;
        public int RemainingDays => IsCurrentlyActive ? (int)(EndDate - DateTime.UtcNow).TotalDays : 0;

        public static Campaign Create(string name, decimal discountPercent, DateTime startDate, DateTime endDate, int companyId, string? description = null, string? bannerImageUrl = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Kampanya adı boş olamaz.", nameof(name));

            if (discountPercent <= 0 || discountPercent > 100)
                throw new ArgumentException("İndirim yüzdesi 0-100 arasında olmalıdır.", nameof(discountPercent));

            if (startDate >= endDate)
                throw new ArgumentException("Başlangıç tarihi bitiş tarihinden önce olmalıdır.", nameof(startDate));

            return new Campaign
            {
                Name = name,
                Description = description,
                BannerImageUrl = bannerImageUrl,
                DiscountPercent = discountPercent,
                StartDate = startDate,
                EndDate = endDate,
                CompanyId = companyId,
                IsActive = true
            };
        }

        public void Update(string name, decimal discountPercent, DateTime startDate, DateTime endDate, string? description = null, string? bannerImageUrl = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Kampanya adı boş olamaz.", nameof(name));

            if (discountPercent <= 0 || discountPercent > 100)
                throw new ArgumentException("İndirim yüzdesi 0-100 arasında olmalıdır.", nameof(discountPercent));

            if (startDate >= endDate)
                throw new ArgumentException("Başlangıç tarihi bitiş tarihinden önce olmalıdır.", nameof(startDate));

            Name = name;
            Description = description;
            BannerImageUrl = bannerImageUrl;
            DiscountPercent = discountPercent;
            StartDate = startDate;
            EndDate = endDate;
            MarkAsModified();
        }

        public void UpdateBannerImage(string? bannerImageUrl)
        {
            BannerImageUrl = bannerImageUrl;
            MarkAsModified();
        }

        public void Activate()
        {
            IsActive = true;
            MarkAsModified();
        }

        public void Deactivate()
        {
            IsActive = false;
            MarkAsModified();
        }
    }
}
