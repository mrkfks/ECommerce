using System.Threading.Tasks;
using ECommerce.Application.DTOs;

namespace ECommerce.Application.Interfaces
{
    public interface ISearchService
    {
        Task<bool> IndexProductAsync(ProductDto product);
        Task<bool> DeleteProductAsync(int productId);
        Task<PaginatedResult<ProductDto>> SearchProductsAsync(string query, int page, int pageSize, int? categoryId = null);
    }
}
