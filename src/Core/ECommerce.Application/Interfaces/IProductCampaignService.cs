using ECommerce.Application.DTOs;

namespace ECommerce.Application.Interfaces;

public interface IProductCampaignService
{
    /// <summary>
    /// Kampanyaya ürün ekle
    /// </summary>
    Task<ProductCampaignDto> AddProductToCampaignAsync(ProductCampaignFormDto dto);

    /// <summary>
    /// Kampanyadan ürünü kaldır
    /// </summary>
    Task RemoveProductFromCampaignAsync(int productId, int campaignId);

    /// <summary>
    /// Belirli bir ürünün aktif kampanyalarını getir
    /// </summary>
    Task<IReadOnlyList<ProductCampaignDto>> GetActiveByProductIdAsync(int productId);

    /// <summary>
    /// Belirli bir kampanyanın ürünlerini getir
    /// </summary>
    Task<IReadOnlyList<ProductCampaignDto>> GetByCampaignIdAsync(int campaignId);

    /// <summary>
    /// Kampanya tarihine göre otomatik uyarlanan fiyatları getir (tüm aktif kampanyalar)
    /// </summary>
    Task<IReadOnlyList<ProductCampaignDto>> GetCurrentActiveCampaignsAsync(int companyId);

    /// <summary>
    /// Kampanya bilgisini güncelle
    /// </summary>
    Task UpdatePricesAsync(int productId, int campaignId, ProductCampaignFormDto dto);

    /// <summary>
    /// Ürün-Kampanya İlişkisini Getir
    /// </summary>
    Task<ProductCampaignDto?> GetByIdAsync(int productId, int campaignId);
}
