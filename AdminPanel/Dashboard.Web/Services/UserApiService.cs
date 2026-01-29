using Dashboard.Web.Models;
using ECommerce.Application.DTOs;
using ECommerce.Application.Responses;

namespace Dashboard.Web.Services
{
    public class UserApiService : ApiService<UserDto>
    {
        public UserApiService(HttpClient httpClient) : base(httpClient)
        {
        }

        public async Task<List<string>> GetRolesAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/Role");
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
                var response = await _httpClient.PostAsJsonAsync("api/User", dto);
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
                var response = await _httpClient.PutAsJsonAsync($"api/User/{id}", dto);
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
                var response = await _httpClient.GetAsync($"api/User?companyId={companyId}");
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
                var response = await _httpClient.GetAsync("api/User/profile");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<UserDto>();
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> UpdateProfileAsync(UserProfileUpdateDto dto)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync("api/User/profile", dto);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
    }
}
