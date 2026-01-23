namespace ECommerce.Domain.Entities
{
    /// <summary>
    /// Ürün varyantları - Öznitelik kombinasyonları
    /// Örnek: iPhone 15 Pro - Siyah - 256GB
    /// </summary>
    public class ProductVariant : BaseEntity, ITenantEntity
    {
        private ProductVariant() { }

        public int ProductId { get; private set; }
        public int CompanyId { get; private set; }
        public string Sku { get; private set; } = string.Empty; // Stock Keeping Unit
        public decimal? PriceAdjustment { get; private set; } // Ana ürün fiyatına ek/indirim
        public int StockQuantity { get; private set; }
        public bool IsActive { get; private set; } = true;

        // Navigation Properties
        public virtual Product Product { get; private set; } = null!;
        public virtual ICollection<ProductVariantAttribute> VariantAttributes { get; private set; } = new List<ProductVariantAttribute>();

        public static ProductVariant Create(int productId, int companyId, string sku, decimal? priceAdjustment = null, int stockQuantity = 0)
        {
            if (string.IsNullOrWhiteSpace(sku))
                throw new ArgumentException("SKU boş olamaz.", nameof(sku));

            if (stockQuantity < 0)
                throw new ArgumentException("Stok miktarı negatif olamaz.", nameof(stockQuantity));

            return new ProductVariant
            {
                ProductId = productId,
                CompanyId = companyId,
                Sku = sku,
                PriceAdjustment = priceAdjustment,
                StockQuantity = stockQuantity,
                IsActive = true
            };
        }

        public void Update(string sku, decimal? priceAdjustment)
        {
            if (string.IsNullOrWhiteSpace(sku))
                throw new ArgumentException("SKU boş olamaz.", nameof(sku));

            Sku = sku;
            PriceAdjustment = priceAdjustment;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateStock(int quantity)
        {
            if (quantity < 0)
                throw new ArgumentException("Stok miktarı negatif olamaz.", nameof(quantity));

            StockQuantity = quantity;
            UpdatedAt = DateTime.UtcNow;
        }

        public void DecreaseStock(int quantity)
        {
            if (quantity <= 0)
                throw new ArgumentException("Azaltılacak miktar sıfırdan büyük olmalıdır.", nameof(quantity));

            if (StockQuantity < quantity)
                throw new InvalidOperationException($"Yeterli stok yok. Mevcut: {StockQuantity}, İstenen: {quantity}");

            StockQuantity -= quantity;
            UpdatedAt = DateTime.UtcNow;
        }

        public void IncreaseStock(int quantity)
        {
            if (quantity <= 0)
                throw new ArgumentException("Artırılacak miktar sıfırdan büyük olmalıdır.", nameof(quantity));

            StockQuantity += quantity;
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

        public decimal GetFinalPrice(decimal basePrice)
        {
            return basePrice + (PriceAdjustment ?? 0);
        }
    }
}
