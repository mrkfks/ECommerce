using ECommerce.Application.DTOs;

namespace Dashboard.Web.Services;

public class CompanyApiService : ApiService<CompanyDto>
{
    public CompanyApiService(IHttpClientFactory httpClientFactory) 
        : base(httpClientFactory)
    {
    }

    public async Task<bool> ApproveAsync(int id)
    {
        try
        {
            var response = await _httpClient.PostAsync($"api/Company/{id}/approve", null);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> DeactivateAsync(int id)
    {
        try
        {
            var response = await _httpClient.PostAsync($"api/Company/{id}/deactivate", null);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> ActivateAsync(int id)
    {
        try
        {
            var response = await _httpClient.PostAsync($"api/Company/{id}/activate", null);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}
