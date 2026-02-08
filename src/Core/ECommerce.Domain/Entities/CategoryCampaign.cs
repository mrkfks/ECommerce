namespace ECommerce.Domain.Entities;

/// <summary>
/// Kategori-Kampanya ilişkisi
/// Bir kategorideki tüm ürünlere belirli bir kampanyanın uygulanmasını tanımlar.
/// </summary>
public class CategoryCampaign : BaseEntity
{
    private CategoryCampaign() { }

    public int CategoryId { get; private set; }
    public int CampaignId { get; private set; }

    // İlişkiler
    public virtual Category? Category { get; private set; }
    public virtual Campaign? Campaign { get; private set; }

    public static CategoryCampaign Create(int categoryId, int campaignId)
    {
        if (categoryId <= 0)
            throw new ArgumentException("Kategori ID geçersiz.", nameof(categoryId));

        if (campaignId <= 0)
            throw new ArgumentException("Kampanya ID geçersiz.", nameof(campaignId));

        return new CategoryCampaign
        {
            CategoryId = categoryId,
            CampaignId = campaignId
        };
    }
}
