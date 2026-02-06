using ECommerce.Application.DTOs;
using ECommerce.Application.Responses;

namespace Dashboard.Web.Services
{
    public class UserApiService : ApiService<UserDto>
    {
        private readonly ILogger<UserApiService> _logger;

        public UserApiService(HttpClient httpClient, ILogger<UserApiService> logger) : base(httpClient)
        {
            _logger = logger;
        }

        public async Task<List<string>> GetRolesAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/roles");
                if (response.IsSuccessStatusCode)
                {
                    var roles = await response.Content.ReadFromJsonAsync<List<RoleDto>>();
                    return roles?.Select(r => r.Name).ToList() ?? new List<string>();
                }
                return new List<string>();
            }
            catch
            {
                return new List<string>();
            }
        }

        public async Task<bool> CreateAsync(UserCreateDto dto)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/users", dto);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateAsync(int id, UserUpdateDto dto)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"api/users/{id}", dto);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<ApiResponse<List<UserDto>>> GetByCompanyAsync(int companyId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/users?companyId={companyId}");
                var content = await response.Content.ReadAsStringAsync();
                var apiResponse = System.Text.Json.JsonSerializer.Deserialize<ApiResponse<List<UserDto>>>(content, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return apiResponse ?? new ApiResponse<List<UserDto>> { Success = false, Message = "Deserialization failed" };
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<UserDto>> { Success = false, Message = ex.Message };
            }
        }

        public async Task<UserDto?> GetProfileAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/users/profile");
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("[UserApiService.GetProfileAsync] Received {Status} from API.", response.StatusCode);
                    return await response.Content.ReadFromJsonAsync<UserDto>();
                }

                var content = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("[UserApiService.GetProfileAsync] API returned {Status}. Content: {Content}", response.StatusCode, content);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[UserApiService.GetProfileAsync] Exception calling api/users/profile");
                return null;
            }
        }

        public async Task<bool> UpdateProfileAsync(UserProfileUpdateDto dto)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync("api/users/profile", dto);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
    }
}
