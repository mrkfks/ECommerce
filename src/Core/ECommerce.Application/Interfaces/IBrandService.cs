using ECommerce.Application.DTOs;

namespace ECommerce.Application.Interfaces
{
    public interface IBrandService
    {
        Task<IReadOnlyList<BrandDto>> GetAllAsync();
        Task<BrandDto?> GetByIdAsync(int id);
        Task<BrandDto> CreateAsync(BrandCreateDto dto);
        Task UpdateAsync(BrandUpdateDto dto);
        Task UpdateImageAsync(int id, string imageUrl);
        Task DeleteAsync(int id);
    }
}
