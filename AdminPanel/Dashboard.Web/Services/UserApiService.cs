using ECommerce.Application.DTOs;

namespace Dashboard.Web.Services
{
    public class UserApiService : ApiService<UserDto>
    {
        private readonly HttpClient _httpClient;

        public UserApiService(HttpClient httpClient) : base(httpClient, "User")
        {
            _httpClient = httpClient;
        }

        public async Task<List<string>> GetRolesAsync()
        {
            try
            {
                var roles = await _httpClient.GetFromJsonAsync<List<string>>("api/User/roles");
                return roles ?? new List<string>();
            }
            catch
            {
                return new List<string>();
            }
        }
    }
}
