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

    public ApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        var name = typeof(T).Name;
        if (name.EndsWith("FormDto")) name = name[..^7];
        else if (name.EndsWith("Dto")) name = name[..^3];
        else if (name.EndsWith("ViewModel")) name = name[..^9];

        _endpoint = ToKebabCase(name);
        // Doğru çoğul ekleme
        if (_endpoint.EndsWith("y"))
            _endpoint = _endpoint[..^1] + "ies";
        else if (!_endpoint.EndsWith("s"))
            _endpoint += "s";
        if (_endpoint == "customer-message") _endpoint = "customer-messages";

        // Endpoint boşsa logla ve exception fırlat
        if (string.IsNullOrWhiteSpace(_endpoint))
        {
            var logPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "logs", "category-endpoint-error.txt");
            File.AppendAllText(logPath, $"[{DateTime.Now}] ApiService endpoint is empty for type: {name}\n");
            throw new InvalidOperationException($"ApiService endpoint is empty for type: {name}");
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

    public async Task<ECommerce.Application.Responses.ApiResponse<List<T>>> GetAllAsync()
    {
        try
        {
            Console.WriteLine($"[ApiService.GetAllAsync] Calling: api/{_endpoint}");
            var response = await _httpClient.GetAsync($"api/{_endpoint}");
            Console.WriteLine($"[ApiService.GetAllAsync] Status: {response.StatusCode}");
            var content = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"[ApiService.GetAllAsync] Response length: {content?.Length ?? 0}");

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"[ApiService.GetAllAsync] ERROR - Status: {response.StatusCode}, Content: {content}");
                return new ECommerce.Application.Responses.ApiResponse<List<T>> { Success = false, Data = new List<T>(), Message = $"API Error: {response.StatusCode}" };
            }

            var apiResponse = JsonSerializer.Deserialize<ECommerce.Application.Responses.ApiResponse<List<T>>>(content ?? "{}", _jsonOptions);
            Console.WriteLine($"[ApiService.GetAllAsync] Deserialized - Success: {apiResponse?.Success}, Count: {apiResponse?.Data?.Count ?? 0}");
            return apiResponse ?? new ECommerce.Application.Responses.ApiResponse<List<T>> { Success = false, Data = new List<T>(), Message = "GetAll failed" };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ApiService.GetAllAsync] EXCEPTION: {ex.Message}");
            return new ECommerce.Application.Responses.ApiResponse<List<T>> { Success = false, Data = new List<T>(), Message = ex.Message };
        }
    }



    public async Task<ECommerce.Application.Responses.PagedResult<T>> GetPagedListAsync(int pageNumber, int pageSize)
    {
        try
        {
            Console.WriteLine($"[ApiService.GetPagedListAsync] Calling: api/{_endpoint}?pageNumber={pageNumber}&pageSize={pageSize}");
            var response = await _httpClient.GetAsync($"api/{_endpoint}?pageNumber={pageNumber}&pageSize={pageSize}");
            Console.WriteLine($"[ApiService.GetPagedListAsync] Status: {response.StatusCode}");

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"[ApiService.GetPagedListAsync] ERROR - Status: {response.StatusCode}");
                return new ECommerce.Application.Responses.PagedResult<T>();
            }

            var content = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"[ApiService.GetPagedListAsync] Response: {content?.Substring(0, Math.Min(500, content?.Length ?? 0))}");

            // Try wrapped ApiResponseDto<PagedResult<T>>
            try
            {
                Console.WriteLine("[ApiService.GetPagedListAsync] Trying wrapped ApiResponseDto<PagedResult<T>>...");
                var wrapped = JsonSerializer.Deserialize<ECommerce.Application.DTOs.ApiResponseDto<ECommerce.Application.Responses.PagedResult<T>>>(content ?? "{}", _jsonOptions);
                Console.WriteLine($"[ApiService.GetPagedListAsync] Wrapped result - Success: {wrapped?.Success}, Data is null: {wrapped?.Data == null}, Items count: {wrapped?.Data?.Items?.Count() ?? 0}");
                if (wrapped?.Success == true && wrapped.Data != null)
                {
                    Console.WriteLine($"[ApiService.GetPagedListAsync] Returning wrapped.Data with {wrapped.Data.Items?.Count() ?? 0} items");
                    return wrapped.Data;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ApiService.GetPagedListAsync] Wrapped deserialization failed: {ex.Message}");
            }

            // Try direct PagedResult<T>
            try
            {
                Console.WriteLine("[ApiService.GetPagedListAsync] Trying direct PagedResult<T>...");
                var result = JsonSerializer.Deserialize<ECommerce.Application.Responses.PagedResult<T>>(content ?? "{}", _jsonOptions);
                Console.WriteLine($"[ApiService.GetPagedListAsync] Direct result - Items count: {result?.Items?.Count() ?? 0}");
                return result ?? new ECommerce.Application.Responses.PagedResult<T>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ApiService.GetPagedListAsync] Direct deserialization failed: {ex.Message}");
            }

            return new ECommerce.Application.Responses.PagedResult<T>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ApiService] GetPagedListAsync exception: {ex.Message}");
            return new ECommerce.Application.Responses.PagedResult<T>();
        }
    }

    public async Task<ECommerce.Application.Responses.ApiResponse<T?>> GetByIdAsync(int id)
    {
        try
        {
            Console.WriteLine($"[ApiService.GetByIdAsync] Endpoint: {_endpoint}, ID: {id}");
            var response = await _httpClient.GetAsync($"api/{_endpoint}/{id}");
            var content = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"[ApiService.GetByIdAsync] Response: {content}");

            var apiResponse = JsonSerializer.Deserialize<ECommerce.Application.Responses.ApiResponse<T>>(content, _jsonOptions);
            if (apiResponse != null)
            {
                Console.WriteLine($"[ApiService.GetByIdAsync] Deserialized: Success={apiResponse.Success}, Data type={apiResponse.Data?.GetType().Name}");
                return new ECommerce.Application.Responses.ApiResponse<T?>
                {
                    Success = apiResponse.Success,
                    Data = apiResponse.Data,
                    Message = apiResponse.Message
                };
            }
            return new ECommerce.Application.Responses.ApiResponse<T?> { Success = false, Data = null, Message = "Not found" };
        }
        catch (Exception ex)
        {
            return new ECommerce.Application.Responses.ApiResponse<T?> { Success = false, Data = null, Message = ex.Message };
        }
    }

    public async Task<ECommerce.Application.Responses.ApiResponse<T>> CreateAsync(T entity)
    {
        try
        {
            Console.WriteLine($"[ApiService.CreateAsync] Calling POST api/{_endpoint}");
            var response = await _httpClient.PostAsJsonAsync($"api/{_endpoint}", entity);
            Console.WriteLine($"[ApiService.CreateAsync] Status: {response.StatusCode}");
            var content = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"[ApiService.CreateAsync] Response: {content?.Substring(0, Math.Min(500, content?.Length ?? 0))}");

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"[ApiService.CreateAsync] ERROR - Status: {response.StatusCode}");
                return new ECommerce.Application.Responses.ApiResponse<T> { Success = false, Message = $"API Error: {response.StatusCode} - {content}" };
            }

            var apiResponse = JsonSerializer.Deserialize<ECommerce.Application.Responses.ApiResponse<T>>(content ?? "{}", _jsonOptions);
            Console.WriteLine($"[ApiService.CreateAsync] Deserialized - Success: {apiResponse?.Success}");
            return apiResponse ?? new ECommerce.Application.Responses.ApiResponse<T> { Success = false, Message = "Create failed" };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ApiService.CreateAsync] EXCEPTION: {ex.Message}");
            return new ECommerce.Application.Responses.ApiResponse<T> { Success = false, Message = ex.Message };
        }
    }

    public async Task<ECommerce.Application.Responses.ApiResponse<T>> UpdateAsync(int id, T entity)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"api/{_endpoint}/{id}", entity);
            var content = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<ECommerce.Application.Responses.ApiResponse<T>>(content, _jsonOptions);
            return apiResponse ?? new ECommerce.Application.Responses.ApiResponse<T> { Success = false, Message = "Update failed" };
        }
        catch (Exception ex)
        {
            return new ECommerce.Application.Responses.ApiResponse<T> { Success = false, Message = ex.Message };
        }
    }



    public async Task<ECommerce.Application.Responses.ApiResponse<bool>> DeleteAsync(int id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"api/{_endpoint}/{id}");
            var content = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<ECommerce.Application.Responses.ApiResponse<bool>>(content, _jsonOptions);
            return apiResponse ?? new ECommerce.Application.Responses.ApiResponse<bool> { Success = false, Message = "Delete failed" };
        }
        catch (Exception ex)
        {
            return new ECommerce.Application.Responses.ApiResponse<bool> { Success = false, Message = ex.Message };
        }
    }


    public async Task<bool> UpdateAsync<TUpdate>(int id, TUpdate entity)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"api/{_endpoint}/{id}", entity);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<ECommerce.Application.Responses.ApiResponse<object>>(content, _jsonOptions);
                return apiResponse?.Success ?? true;
            }
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ApiService] UpdateAsync<{typeof(TUpdate).Name}> exception: {ex.Message}");
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

    public async Task<TResponse?> GetAsync<TResponse>(string subUrl)
    {
        try
        {
            var url = subUrl.StartsWith("api/") ? subUrl : $"api/{_endpoint}/{subUrl}";
            var response = await _httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<ECommerce.Application.Responses.ApiResponse<TResponse>>(content, _jsonOptions);
                return apiResponse != null ? apiResponse.Data : default;
            }
            return default;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ApiService] GetAsync<{typeof(TResponse).Name}> exception: {ex.Message}");
            return default;
        }
    }

    public async Task<List<T>> GetListAsync(string subUrl)
    {
        try
        {
            var url = subUrl.StartsWith("api/") ? subUrl : $"api/{_endpoint}/{subUrl}";
            var response = await _httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<ECommerce.Application.Responses.ApiResponse<List<T>>>(content, _jsonOptions);
                return apiResponse?.Data ?? new List<T>();
            }
            return new List<T>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ApiService] GetListAsync exception: {ex.Message}");
            return new List<T>();
        }
    }
}
