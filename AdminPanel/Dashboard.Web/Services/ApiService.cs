using System.Net.Http.Json;
using System.Text.Json;
using ECommerce.Application.Responses;

namespace Dashboard.Web.Services;



public class ApiService<T> : IApiService<T> where T : class
{
    protected readonly HttpClient _httpClient;
    protected readonly string _endpoint;
    private static readonly JsonSerializerOptions _jsonOptions = new() 
    { 
        PropertyNameCaseInsensitive = true 
    };

    public ApiService(IHttpClientFactory httpClientFactory) : this(httpClientFactory, null) { }

    public ApiService(IHttpClientFactory httpClientFactory, string? customEndpoint)
    {
        _httpClient = httpClientFactory.CreateClient("ECommerceApi");
        
        if (!string.IsNullOrEmpty(customEndpoint))
        {
            _endpoint = customEndpoint;
        }
        else
        {
            var name = typeof(T).Name;
            if (name.EndsWith("Dto")) name = name[..^3];
            else if (name.EndsWith("ViewModel")) name = name[..^9];
            
            // Basic pluralization attempt or just use the name
            // For this project, most are simple: Products, Categories, Brands, etc.
            // But some are CustomerMessages (with s). 
            // The API uses: products, categories, customers, customer-messages
            
            _endpoint = ToKebabCase(name);
            if (!_endpoint.EndsWith("s")) _endpoint += "s";
            
            // Special cases
            if (_endpoint == "customer-message") _endpoint = "customer-messages";
        }
    }

    private static string ToKebabCase(string str)
    {
        if (string.IsNullOrEmpty(str)) return str;
        var result = new System.Text.StringBuilder();
        for (int i = 0; i < str.Length; i++)
        {
            if (char.IsUpper(str[i]))
            {
                if (i > 0) result.Append('-');
                result.Append(char.ToLower(str[i]));
            }
            else
            {
                result.Append(str[i]);
            }
        }
        return result.ToString();
    }

    public async Task<List<T>> GetAllAsync()
    {
        return await GetListAsync($"api/{_endpoint}");
    }

    public async Task<List<T>> GetListAsync(string subUrl)
    {
        try
        {
            // If subUrl is relative (e.g. "brand/5"), prepend "api/{endpoint}/" ?
            // The interface says GetListAsync(subUrl).
            // If user passes "brand/5", intention is "api/Model/brand/5".
            // If user passes "api/Items", intention is "api/Items".
            // Let's standardise: Helper method calls usually target the entity endpoint.
            
            // If call is internal GetAllAsync, we passed full path.
            // If call is external like GetListAsync("brand/5"), we probably want "api/{_endpoint}/brand/5".
            
            string url = subUrl.StartsWith("api/") ? subUrl : $"api/{_endpoint}/{subUrl}";

            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                // Console.WriteLine($"[ApiService] GetListAsync failed: {url} {response.StatusCode}");
                return new List<T>();
            }

            var content = await response.Content.ReadAsStringAsync();
            
            // Try ApiResponse<List<T>>
            try
            {
                var apiResponse = JsonSerializer.Deserialize<ECommerce.Application.Responses.ApiResponse<List<T>>>(content, _jsonOptions);
                if (apiResponse?.Success == true && apiResponse.Data != null)
                {
                    return apiResponse.Data;
                }
            }
            catch { }

            // Try List<T>
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
            Console.WriteLine($"[ApiService] GetListAsync exception: {ex.Message}");
            return new List<T>();
        }
    }

    public async Task<ECommerce.Application.Responses.PagedResult<T>> GetPagedListAsync(int pageNumber, int pageSize)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/{_endpoint}?pageNumber={pageNumber}&pageSize={pageSize}");
            if (!response.IsSuccessStatusCode)
            {
                return new ECommerce.Application.Responses.PagedResult<T>();
            }

            var content = await response.Content.ReadAsStringAsync();
            
            // Try wrapped ApiResponseDto<PagedResult<T>>
            try
            {
                var wrapped = JsonSerializer.Deserialize<ECommerce.Application.DTOs.Common.ApiResponseDto<ECommerce.Application.Responses.PagedResult<T>>>(content, _jsonOptions);
                if (wrapped?.Success == true && wrapped.Data != null)
                {
                    return wrapped.Data;
                }
            }
            catch { }

            // Try direct PagedResult<T>
            try
            {
                var result = JsonSerializer.Deserialize<ECommerce.Application.Responses.PagedResult<T>>(content, _jsonOptions);
                return result ?? new ECommerce.Application.Responses.PagedResult<T>();
            }
            catch { }

            return new ECommerce.Application.Responses.PagedResult<T>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ApiService] GetPagedListAsync exception: {ex.Message}");
            return new ECommerce.Application.Responses.PagedResult<T>();
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
                var apiResponse = JsonSerializer.Deserialize<ECommerce.Application.Responses.ApiResponse<T>>(content, _jsonOptions);
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

    public async Task<bool> UpdateAsync<TUpdate>(int id, TUpdate entity)
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

    public async Task<bool> PostActionAsync<TPayload>(string subUrl, TPayload payload)
    {
        try
        {
            var url = subUrl.StartsWith("api/") ? subUrl : $"api/{_endpoint}/{subUrl}";
            var response = await _httpClient.PostAsJsonAsync(url, payload);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ApiService] PostActionAsync exception: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> PutActionAsync<TPayload>(string subUrl, TPayload payload)
    {
        try
        {
            var url = subUrl.StartsWith("api/") ? subUrl : $"api/{_endpoint}/{subUrl}";
            var response = await _httpClient.PutAsJsonAsync(url, payload);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ApiService] PutActionAsync exception: {ex.Message}");
            return false;
        }
    }
}
