using ECommerce.Application.DTOs;

namespace ECommerce.Application.Interfaces
{
    public interface ICategoryService
    {
        Task<IReadOnlyList<CategoryDto>> GetAllAsync();
        Task<CategoryDto?> GetByIdAsync(int id);
        Task<CategoryDto> CreateAsync(CategoryFormDto dto);
        Task UpdateAsync(CategoryFormDto dto);
        Task UpdateImageAsync(int id, string imageUrl);
        Task DeleteAsync(int id);
    }
}
