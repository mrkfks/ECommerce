namespace ECommerce.Domain.Entities
{
    /// <summary>
    /// Varyant ile öznitelik değeri arasındaki ilişki (Many-to-Many)
    /// Örnek: Varyant #1 -> Renk: Kırmızı, Beden: L
    /// </summary>
    public class ProductVariantAttribute
    {
        private ProductVariantAttribute() { }

        public int Id { get; private set; }
        public int ProductVariantId { get; private set; }
        public int AttributeValueId { get; private set; }

        // Navigation Properties
        public virtual ProductVariant? ProductVariant { get; private set; }
        public virtual AttributeValue? AttributeValue { get; private set; }

        public static ProductVariantAttribute Create(int productVariantId, int attributeValueId)
        {
            if (productVariantId <= 0)
                throw new ArgumentException("Geçerli bir varyant seçilmelidir.", nameof(productVariantId));

            if (attributeValueId <= 0)
                throw new ArgumentException("Geçerli bir öznitelik değeri seçilmelidir.", nameof(attributeValueId));

            return new ProductVariantAttribute
            {
                ProductVariantId = productVariantId,
                AttributeValueId = attributeValueId
            };
        }
    }
}
