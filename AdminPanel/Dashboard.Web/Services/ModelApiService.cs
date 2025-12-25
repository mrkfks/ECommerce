using ECommerce.Application.DTOs;

namespace Dashboard.Web.Services
{
    public class ModelApiService : ApiService<ModelDto>
    {
        public ModelApiService(HttpClient httpClient) : base(httpClient, "Model")
        {
        }

        public async Task<List<ModelDto>> GetByBrandIdAsync(int brandId)
        {
            var response = await _httpClient.GetAsync($"{_endpoint}/brand/{brandId}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<ModelDto>>() ?? new();
        }
    }
}
