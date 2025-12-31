using ECommerce.Application.DTOs;

namespace Dashboard.Web.Services;

public class GlobalAttributeApiService : ApiService<GlobalAttributeDto>
{
    public GlobalAttributeApiService(HttpClient httpClient)
        : base(httpClient, "GlobalAttribute")
    {
    }

    public async Task<bool> CreateAsync(GlobalAttributeCreateDto dto)
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

    public async Task<bool> UpdateAsync(int id, GlobalAttributeUpdateDto dto)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"api/{_endpoint}/{id}", dto);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        return await base.DeleteAsync(id);
    }
}
