using ECommerce.Application.DTOs;

namespace ECommerce.Application.Interfaces
{
    public interface IModelService
    {
        Task<IReadOnlyList<ModelDto>> GetAllAsync();
        Task<IReadOnlyList<ModelDto>> GetByBrandIdAsync(int brandId);
        Task<ModelDto?> GetByIdAsync(int id);
        Task<ModelDto> CreateAsync(ModelFormDto dto);
        Task UpdateAsync(ModelFormDto dto);
        Task DeleteAsync(int id);
    }
}
