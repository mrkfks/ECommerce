using System.Net.Http.Json;

namespace Dashboard.Web.Services;

public class ApiService<T> : IApiService<T> where T : class
{
    private readonly HttpClient _httpClient;
    private readonly string _endpoint;

    public ApiService(HttpClient httpClient, string endpoint)
    {
        _httpClient = httpClient;
        _endpoint = endpoint;
    }

    public async Task<List<T>> GetAllAsync()
    {
        try
        {
            var result = await _httpClient.GetFromJsonAsync<List<T>>($"api/{_endpoint}");
            return result ?? new List<T>();
        }
        catch
        {
            return new List<T>();
        }
    }

    public async Task<T?> GetByIdAsync(int id)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<T>($"api/{_endpoint}/{id}");
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> CreateAsync(T entity)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync($"api/{_endpoint}", entity);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> UpdateAsync(int id, T entity)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"api/{_endpoint}/{id}", entity);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"api/{_endpoint}/{id}");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}
