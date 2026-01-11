namespace ECommerce.Domain.Entities
{
    public class ProductImage : BaseEntity
    {
        private ProductImage() { }

        public int ProductId { get; private set; }
        public string ImageUrl { get; private set; } = string.Empty;
        public int Order { get; private set; }
        public bool IsPrimary { get; private set; }

        public virtual Product? Product { get; private set; }

        public static ProductImage Create(int productId, string imageUrl, int order = 0, bool isPrimary = false)
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
                throw new ArgumentException("Resim URL'si boş olamaz.", nameof(imageUrl));

            return new ProductImage
            {
                ProductId = productId,
                ImageUrl = imageUrl,
                Order = order,
                IsPrimary = isPrimary
            };
        }

        public void Update(string imageUrl, int order, bool isPrimary)
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
                throw new ArgumentException("Resim URL'si boş olamaz.", nameof(imageUrl));

            ImageUrl = imageUrl;
            Order = order;
            IsPrimary = isPrimary;
            MarkAsModified();
        }

        public void SetAsPrimary()
        {
            IsPrimary = true;
            MarkAsModified();
        }

        public void UnsetPrimary()
        {
            IsPrimary = false;
            MarkAsModified();
        }

        public void SetOrder(int order)
        {
            Order = order;
            MarkAsModified();
        }
    }
}
