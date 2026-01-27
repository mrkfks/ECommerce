using ECommerce.Application.Responses;

namespace Dashboard.Web.Services;

public interface IApiService<T> where T : class
{
    Task<ApiResponse<List<T>>> GetAllAsync();
    Task<ApiResponse<T?>> GetByIdAsync(int id);
    Task<ApiResponse<T>> CreateAsync(T entity);
    Task<ApiResponse<T>> UpdateAsync(int id, T entity);
    Task<ApiResponse<bool>> DeleteAsync(int id);
    Task<PagedResult<T>> GetPagedListAsync(int pageNumber, int pageSize);
}
using ECommerce.Application.Responses;

namespace Dashboard.Web.Services;

public interface IApiService<T> where T : class
{
    Task<ApiResponse<List<T>>> GetAllAsync();
    Task<ApiResponse<T?>> GetByIdAsync(int id);
    Task<ApiResponse<T>> CreateAsync(T entity);
    Task<ApiResponse<T>> UpdateAsync(int id, T entity);
    Task<ApiResponse<bool>> DeleteAsync(int id);
    Task<PagedResult<T>> GetPagedListAsync(int pageNumber, int pageSize);
}
