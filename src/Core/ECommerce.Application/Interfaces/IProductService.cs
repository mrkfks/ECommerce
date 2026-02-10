using ECommerce.Application.DTOs;

namespace ECommerce.Application.Interfaces
{
    public interface IProductService
    {
        // Tek ürün bilgisi getir
        Task<ProductDto?> GetByIdAsync(int id);

        // Tüm ürünleri getir
        Task<IReadOnlyList<ProductDto>> GetAllAsync();

        // Sayfalanmış ürün listesi getir (Veritabanı seviyesinde)
        Task<ECommerce.Application.Responses.PagedResult<ProductDto>> GetPagedAsync(int pageNumber, int pageSize);

        // Belirli bir kategoriye ait ürünleri getir
        Task<IReadOnlyList<ProductDto>> GetByCategoryIdAsync(int categoryId);

        // Belirli bir markaya ait ürünleri getir
        Task<IReadOnlyList<ProductDto>> GetByBrandIdAsync(int brandId);

        // Şirkete ait ürünleri getir
        Task<IReadOnlyList<ProductDto>> GetByCompanyAsync(int companyId);

        // Yeni ürün oluştur
        Task<ProductDto> CreateAsync(ProductFormDto dto);

        // Mevcut ürünü güncelle
        Task UpdateAsync(ProductFormDto dto);

        // Ürünü sil
        Task DeleteAsync(int id);

        // Stok güncelle
        Task UpdateStockAsync(int productId, int newQuantity);

        // Güvenli stok düşümü (Atomik)
        Task DecreaseStockAsync(int productId, int quantity);

        // Ürün arama (isim veya açıklama üzerinden)
        Task<IReadOnlyList<ProductDto>> SearchAsync(string keyword);

        // Resim işlemleri
        Task<ProductImageDto> AddImageAsync(int productId, string imageUrl, int order, bool isPrimary);
        Task UpdateImageAsync(int productId, int imageId, string imageUrl, int order, bool isPrimary);
        Task RemoveImageAsync(int productId, int imageId);
        Task<List<ProductImageDto>> GetImagesAsync(int productId);

        // Toplu Fiyat Güncelleme
        Task BulkUpdatePriceAsync(List<int> productIds, decimal percentage);

        // Öne çıkan ürünler
        Task<IReadOnlyList<ProductDto>> GetFeaturedAsync(int count = 8);

        // Yeni ürünler
        Task<IReadOnlyList<ProductDto>> GetNewArrivalsAsync(int count = 8);

        // Çok satanlar
        Task<IReadOnlyList<ProductDto>> GetBestSellersAsync(int count = 8);
    }
}

