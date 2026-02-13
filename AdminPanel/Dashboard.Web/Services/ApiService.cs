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
            var response = await _httpClient.GetAsync($"api/{_endpoint}");
            var content = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                return new ECommerce.Application.Responses.ApiResponse<List<T>> { Success = false, Data = new List<T>(), Message = $"API Error: {response.StatusCode}" };
            }

            var apiResponse = JsonSerializer.Deserialize<ECommerce.Application.Responses.ApiResponse<List<T>>>(content ?? "{}", _jsonOptions);
            return apiResponse ?? new ECommerce.Application.Responses.ApiResponse<List<T>> { Success = false, Data = new List<T>(), Message = "GetAll failed" };
        }
        catch (Exception)
        {
            return new ECommerce.Application.Responses.ApiResponse<List<T>> { Success = false, Data = new List<T>(), Message = "An error occurred." };
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
                var wrapped = JsonSerializer.Deserialize<ECommerce.Application.DTOs.ApiResponseDto<ECommerce.Application.Responses.PagedResult<T>>>(content ?? "{}", _jsonOptions);
                if (wrapped?.Success == true && wrapped.Data != null)
                {
                    return wrapped.Data;
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Exception caught.");
            }

            // Try direct PagedResult<T>
            try
            {
                var result = JsonSerializer.Deserialize<ECommerce.Application.Responses.PagedResult<T>>(content ?? "{}", _jsonOptions);
                return result ?? new ECommerce.Application.Responses.PagedResult<T>();
            }
            catch (Exception)
            {
                Console.WriteLine("Exception caught.");
            }

            return new ECommerce.Application.Responses.PagedResult<T>();
        }
        catch (Exception)
        {
            Console.WriteLine("Exception caught.");
            return new ECommerce.Application.Responses.PagedResult<T>();
        }
    }

    public async Task<ECommerce.Application.Responses.ApiResponse<T?>> GetByIdAsync(int id)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/{_endpoint}/{id}");
            var content = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<ECommerce.Application.Responses.ApiResponse<T>>(content, _jsonOptions);
            if (apiResponse != null)
            {
                return new ECommerce.Application.Responses.ApiResponse<T?>
                {
                    Success = apiResponse.Success,
                    Data = apiResponse.Data,
                    Message = apiResponse.Message
                };
            }
            return new ECommerce.Application.Responses.ApiResponse<T?> { Success = false, Data = null, Message = "Not found" };
        }
        catch (Exception)
        {
            return new ECommerce.Application.Responses.ApiResponse<T?> { Success = false, Data = null, Message = "An error occurred." };
        }
    }

    public async Task<ECommerce.Application.Responses.ApiResponse<T>> CreateAsync(T entity)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync($"api/{_endpoint}", entity);
            var content = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                return new ECommerce.Application.Responses.ApiResponse<T> { Success = false, Message = $"API Error: {response.StatusCode} - {content}" };
            }

            var apiResponse = JsonSerializer.Deserialize<ECommerce.Application.Responses.ApiResponse<T>>(content ?? "{}", _jsonOptions);
            return apiResponse ?? new ECommerce.Application.Responses.ApiResponse<T> { Success = false, Message = "Create failed" };
        }
        catch (Exception)
        {
            return new ECommerce.Application.Responses.ApiResponse<T> { Success = false, Message = "An error occurred." };
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
        catch (Exception)
        {
            return new ECommerce.Application.Responses.ApiResponse<T> { Success = false, Message = "An error occurred." };
        }
    }



    public async Task<ECommerce.Application.Responses.ApiResponse<bool>> DeleteAsync(int id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"api/{_endpoint}/{id}");
            var content = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<ECommerce.Application.Responses.ApiResponse<bool>>(content, _jsonOptions);
            return apiResponse ?? new ECommerce.Application.Responses.ApiResponse<bool> { Success = false, Message = "An error occurred." };
        }

        catch (Exception)
        {
            Console.WriteLine("Exception caught.");
            return new ECommerce.Application.Responses.ApiResponse<bool> { Success = false, Message = "An error occurred." };
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
        catch (Exception)
        {
            Console.WriteLine("Exception caught.");
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
        catch (Exception)
        {
            Console.WriteLine("Exception caught.");
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
        catch (Exception)
        {
            Console.WriteLine("Exception caught.");
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
        catch (Exception)
        {
            Console.WriteLine("Exception caught.");
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
        catch (Exception)
        {
            return new List<T>();
        }
    }
}
