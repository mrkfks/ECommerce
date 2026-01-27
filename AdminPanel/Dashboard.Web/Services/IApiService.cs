using ECommerce.Application.Responses;

namespace Dashboard.Web.Services;

public interface IApiService<T> where T : class
{
    Task<ApiResponse<List<T>>> GetAllAsync();
    Task<ApiResponse<T?>> GetByIdAsync(int id);
    Task<ApiResponse<T>> CreateAsync(T entity);
    Task<bool> CreateAsync<TCreate>(TCreate entity);
    Task<ApiResponse<T>> UpdateAsync(int id, T entity);
    Task<bool> UpdateAsync<TUpdate>(int id, TUpdate entity);
    Task<ApiResponse<bool>> DeleteAsync(int id);
    Task<PagedResult<T>> GetPagedListAsync(int pageNumber, int pageSize);
    Task<List<T>> GetListAsync(string subUrl);
    Task<bool> PostActionAsync<TPayload>(string subUrl, TPayload payload);
    Task<bool> PutActionAsync<TPayload>(string subUrl, TPayload payload);
    Task<TResponse?> GetAsync<TResponse>(string subUrl);
}
