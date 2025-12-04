using ECommerce.Domain.Entities;

namespace ECommerce.Domain.Interfaces
{
    public interface IProductRepository : IGenericRepository<Product>
    {
        Task<IReadOnlyList<Product>> GetPageAsync(int page, int pageSize, int? categoryId, int? brandId, string? search);
        Task<bool> IsStockAvailableAsync(int productId, int quantity);
    }
}