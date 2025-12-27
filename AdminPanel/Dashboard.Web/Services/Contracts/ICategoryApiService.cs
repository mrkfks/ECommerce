using ECommerce.Application.DTOs;

namespace Dashboard.Web.Services.Contracts;

/// <summary>
/// Kategori yönetimi için servis arayüzü
/// </summary>
public interface ICategoryApiService
{
    Task<IEnumerable<CategoryDto>> GetAllAsync();
    Task<CategoryDto?> GetByIdAsync(int id);
    Task<CategoryDto?> CreateAsync(CategoryCreateDto dto);
    Task<bool> UpdateAsync(int id, CategoryUpdateDto dto);
    Task<bool> DeleteAsync(int id);
}
