using ECommerce.Application.DTOs;

namespace ECommerce.Application.Interfaces
{
    public interface IBrandService
    {
        Task<IReadOnlyList<BrandDto>> GetAllAsync();
        Task<BrandDto?> GetByIdAsync(int id);
        Task<BrandDto> CreateAsync(BrandFormDto dto);
        Task UpdateAsync(BrandFormDto dto);
        Task UpdateImageAsync(int id, string imageUrl);
        Task DeleteAsync(int id);
    }
}
