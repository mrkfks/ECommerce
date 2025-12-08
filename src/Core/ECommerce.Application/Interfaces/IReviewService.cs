using ECommerce.Application.DTOs;

namespace ECommerce.Application.Interfaces
{
    public interface IReviewService
    {
        // Tek yorum bilgisi getir
        Task<ReviewDto?> GetByIdAsync(int id);

        // Tüm yorumları getir
        Task<IReadOnlyList<ReviewDto>> GetAllAsync();

        // Belirli bir ürüne ait yorumları getir
        Task<IReadOnlyList<ReviewDto>> GetByProductIdAsync(int productId);

        // Belirli bir müşteriye ait yorumları getir
        Task<IReadOnlyList<ReviewDto>> GetByCustomerIdAsync(int customerId);

        // Yeni yorum oluştur
        Task<ReviewDto> CreateAsync(ReviewCreateDto dto);

        // Mevcut yorumu güncelle
        Task UpdateAsync(ReviewUpdateDto dto);

        // Yorumu sil
        Task DeleteAsync(int id);

        // Ortalama puanı getir (ürün bazlı)
        Task<double> GetAverageRatingAsync(int productId);
    }
}
