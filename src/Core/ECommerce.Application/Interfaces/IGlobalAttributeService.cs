using ECommerce.Application.DTOs;

namespace ECommerce.Application.Interfaces
{
    public interface IGlobalAttributeService
    {
        Task<IReadOnlyList<GlobalAttributeDto>> GetAllAsync();
        Task<GlobalAttributeDto?> GetByIdAsync(int id);
        Task<GlobalAttributeDto> CreateAsync(GlobalAttributeFormDto dto);
        Task UpdateAsync(GlobalAttributeFormDto dto);
        Task DeleteAsync(int id);
    }
}
