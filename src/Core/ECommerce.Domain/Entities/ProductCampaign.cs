namespace ECommerce.Domain.Entities;

/// <summary>
/// Ürün-Kampanya ilişkisi
/// Bir ürüne belirli bir tarih aralığında belirli bir kampanyanın uygulanmasını tanımlar.
/// </summary>
public class ProductCampaign : BaseEntity
{
    private ProductCampaign() { }

    public int ProductId { get; private set; }
    public int CampaignId { get; private set; }
    public decimal OriginalPrice { get; private set; }
    public decimal DiscountedPrice { get; private set; }

    // İlişkiler
    public virtual Product? Product { get; private set; }
    public virtual Campaign? Campaign { get; private set; }

    public static ProductCampaign Create(int productId, int campaignId, decimal originalPrice, decimal discountedPrice)
    {
        if (originalPrice <= 0)
            throw new ArgumentException("Orijinal fiyat 0'dan büyük olmalıdır.", nameof(originalPrice));

        if (discountedPrice < 0)
            throw new ArgumentException("İndirimli fiyat 0 veya daha büyük olmalıdır.", nameof(discountedPrice));

        if (discountedPrice >= originalPrice)
            throw new ArgumentException("İndirimli fiyat orijinal fiyattan küçük olmalıdır.", nameof(discountedPrice));

        return new ProductCampaign
        {
            ProductId = productId,
            CampaignId = campaignId,
            OriginalPrice = originalPrice,
            DiscountedPrice = discountedPrice
        };
    }

    public void UpdatePrices(decimal originalPrice, decimal discountedPrice)
    {
        if (originalPrice <= 0)
            throw new ArgumentException("Orijinal fiyat 0'dan büyük olmalıdır.", nameof(originalPrice));

        if (discountedPrice < 0)
            throw new ArgumentException("İndirimli fiyat 0 veya daha büyük olmalıdır.", nameof(discountedPrice));

        if (discountedPrice >= originalPrice)
            throw new ArgumentException("İndirimli fiyat orijinal fiyattan küçük olmalıdır.", nameof(discountedPrice));

        OriginalPrice = originalPrice;
        DiscountedPrice = discountedPrice;
        MarkAsModified();
    }

    /// <summary>
    /// İndirim yüzdesini hesapla
    /// </summary>
    public decimal GetDiscountPercentage()
    {
        if (OriginalPrice == 0) return 0;
        return ((OriginalPrice - DiscountedPrice) / OriginalPrice) * 100;
    }
}
