namespace Dashboard.Web.Services;

public interface IApiService<T> where T : class
{
    Task<List<T>> GetAllAsync();
    Task<List<T>> GetListAsync(string subUrl);
    Task<ECommerce.Application.Responses.PagedResult<T>> GetPagedListAsync(int pageNumber, int pageSize);
    Task<T?> GetByIdAsync(int id);
    Task<bool> CreateAsync(T entity);
    Task<bool> CreateAsync<TCreate>(TCreate entity);
    Task<bool> UpdateAsync(int id, T entity);
    Task<bool> UpdateAsync<TUpdate>(int id, TUpdate entity);
    Task<bool> DeleteAsync(int id);
    Task<bool> PostActionAsync<TPayload>(string subUrl, TPayload payload);
    Task<bool> PutActionAsync<TPayload>(string subUrl, TPayload payload);
}
