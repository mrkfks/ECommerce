using ECommerce.Application.DTOs;
using System.Net.Http.Json;

namespace Dashboard.Web.Services
{
    public class ModelApiService : ApiService<ModelDto>
    {
        public ModelApiService(HttpClient httpClient) : base(httpClient, "Model")
        {
        }

        public async Task<List<ModelDto>> GetByBrandIdAsync(int brandId)
        {
            var response = await _httpClient.GetAsync($"api/{_endpoint}/brand/{brandId}");
            if (!response.IsSuccessStatusCode)
                return new List<ModelDto>();
            return await response.Content.ReadFromJsonAsync<List<ModelDto>>() ?? new();
        }

        public async Task<bool> CreateModelAsync(ModelCreateDto dto)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"api/{_endpoint}", dto);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
    }
}
