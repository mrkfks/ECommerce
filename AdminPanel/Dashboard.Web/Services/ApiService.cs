using System.Net.Http.Json;
using System.Text.Json;

namespace Dashboard.Web.Services;

// API'nin döndürdüğü wrapper response
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
}

public class ApiService<T> : IApiService<T> where T : class
{
    protected readonly HttpClient _httpClient;
    protected readonly string _endpoint;
    private static readonly JsonSerializerOptions _jsonOptions = new() 
    { 
        PropertyNameCaseInsensitive = true 
    };

    public ApiService(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("ECommerceApi");
        _endpoint = typeof(T).Name.Replace("Dto", "");
    }

    public async Task<List<T>> GetAllAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/{_endpoint}");
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"[ApiService] GetAllAsync failed: {response.StatusCode}");
                return new List<T>();
            }

            var content = await response.Content.ReadAsStringAsync();
            
            // Önce ApiResponse<List<T>> olarak dene
            try
            {
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<List<T>>>(content, _jsonOptions);
                if (apiResponse?.Success == true && apiResponse.Data != null)
                {
                    Console.WriteLine($"[ApiService] GetAllAsync: {apiResponse.Data.Count} items");
                    return apiResponse.Data;
                }
            }
            catch { }

            // Düz List<T> olarak dene
            try
            {
                var result = JsonSerializer.Deserialize<List<T>>(content, _jsonOptions);
                return result ?? new List<T>();
            }
            catch { }

            return new List<T>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ApiService] GetAllAsync exception: {ex.Message}");
            return new List<T>();
        }
    }

    public async Task<T?> GetByIdAsync(int id)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/{_endpoint}/{id}");
            if (!response.IsSuccessStatusCode)
                return null;

            var content = await response.Content.ReadAsStringAsync();
            
            // Önce ApiResponse<T> olarak dene
            try
            {
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<T>>(content, _jsonOptions);
                if (apiResponse?.Success == true && apiResponse.Data != null)
                    return apiResponse.Data;
            }
            catch { }

            // Düz T olarak dene
            try
            {
                return JsonSerializer.Deserialize<T>(content, _jsonOptions);
            }
            catch { }

            return null;
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

    // Generic Create support for different DTOs (e.g. ProductCreateDto -> ProductDto)
    public async Task<bool> CreateAsync<TCreate>(TCreate entity)
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
}
