using ECommerce.Application.DTOs;

namespace ECommerce.Application.Interfaces
{
    public interface IGlobalAttributeService
    {
        Task<IReadOnlyList<GlobalAttributeDto>> GetAllAsync();
        Task<GlobalAttributeDto?> GetByIdAsync(int id);
        Task<GlobalAttributeDto> CreateAsync(GlobalAttributeCreateDto dto);
        Task UpdateAsync(GlobalAttributeUpdateDto dto);
        Task DeleteAsync(int id);
    }
}
