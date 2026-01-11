using Dashboard.Web.Models;
using System.Text.Json;

namespace Dashboard.Web.Services
{
    public class BannerApiService
    {
        private readonly HttpClient _httpClient;

        public BannerApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<BannerViewModel>?> GetAllAsync()
        {
            var response = await _httpClient.GetAsync("banner");
            response.EnsureSuccessStatusCode();
            
            var json = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<ApiResponse<List<BannerViewModel>>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return apiResponse?.Data;
        }

        public async Task<BannerViewModel?> GetByIdAsync(int id)
        {
            var response = await _httpClient.GetAsync($"banner/{id}");
            response.EnsureSuccessStatusCode();
            
            var json = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<ApiResponse<BannerViewModel>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return apiResponse?.Data;
        }

        public async Task<bool> CreateAsync(BannerViewModel model)
        {
            var createDto = new
            {
                title = model.Title,
                description = model.Description,
                imageUrl = model.ImageUrl,
                link = model.Link,
                order = model.Order
            };

            var response = await _httpClient.PostAsJsonAsync("banner", createDto);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateAsync(int id, BannerViewModel model)
        {
            var updateDto = new
            {
                title = model.Title,
                description = model.Description,
                imageUrl = model.ImageUrl,
                link = model.Link,
                order = model.Order,
                isActive = model.IsActive
            };

            var response = await _httpClient.PutAsJsonAsync($"banner/{id}", updateDto);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"banner/{id}");
            return response.IsSuccessStatusCode;
        }

        private class ApiResponse<T>
        {
            public bool Success { get; set; }
            public string? Message { get; set; }
            public T? Data { get; set; }
        }
    }
}
