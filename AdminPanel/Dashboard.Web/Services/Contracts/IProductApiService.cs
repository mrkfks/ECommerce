using ECommerce.Application.DTOs;

namespace Dashboard.Web.Services.Contracts;

/// <summary>
/// Ürün yönetimi için servis arayüzü
/// </summary>
public interface IProductApiService
{
    Task<IEnumerable<ProductDto>> GetAllAsync();
    Task<ProductDto?> GetByIdAsync(int id);
    Task<ProductDto?> CreateAsync(ProductCreateDto dto);
    Task<bool> UpdateAsync(int id, ProductUpdateDto dto);
    Task<bool> DeleteAsync(int id);
    Task<IEnumerable<ProductDto>> GetByCategoryAsync(int categoryId);
    Task<IEnumerable<ProductDto>> GetByBrandAsync(int brandId);
}
