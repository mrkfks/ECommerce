using ECommerce.Application.DTOs;

namespace ECommerce.Application.Interfaces
{
    public interface IProductService
    {
        // Tek ürün bilgisi getir
        Task<ProductDto?> GetByIdAsync(int id);

        // Tüm ürünleri getir
        Task<IReadOnlyList<ProductDto>> GetAllAsync();

        // Belirli bir kategoriye ait ürünleri getir
        Task<IReadOnlyList<ProductDto>> GetByCategoryIdAsync(int categoryId);

        // Belirli bir markaya ait ürünleri getir
        Task<IReadOnlyList<ProductDto>> GetByBrandIdAsync(int brandId);

        // Yeni ürün oluştur
        Task<ProductDto> CreateAsync(ProductCreateDto dto);

        // Mevcut ürünü güncelle
        Task UpdateAsync(ProductUpdateDto dto);

        // Ürünü sil
        Task DeleteAsync(int id);

        // Stok güncelle
        Task UpdateStockAsync(int productId, int newQuantity);

        // Ürün arama (isim veya açıklama üzerinden)
        Task<IReadOnlyList<ProductDto>> SearchAsync(string keyword);
    }
}

