using ECommerce.Application.DTOs;

namespace ECommerce.Application.Interfaces
{
    public interface IModelService
    {
        Task<IReadOnlyList<ModelDto>> GetAllAsync();
        Task<IReadOnlyList<ModelDto>> GetByBrandIdAsync(int brandId);
        Task<ModelDto?> GetByIdAsync(int id);
        Task<ModelDto> CreateAsync(ModelCreateDto dto);
        Task UpdateAsync(ModelUpdateDto dto);
        Task DeleteAsync(int id);
    }
}
