using System.Net.Http.Json;
using Dashboard.Web.Models;

namespace Dashboard.Web.Services;

/// <summary>
/// Özellik yönetimi API servisi
/// </summary>
public class FeatureApiService
{
    private readonly HttpClient _httpClient;
    private readonly string _endpoint = "features";

    public FeatureApiService(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("ECommerceApi");
    }

    public async Task<List<FeatureViewModel>?> GetAllAsync()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<List<FeatureViewModel>>($"api/{_endpoint}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[FeatureApiService] Error: {ex.Message}");
            return null;
        }
    }

    public async Task<FeatureViewModel?> GetByIdAsync(int id)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<FeatureViewModel>($"api/{_endpoint}/{id}");
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> CreateAsync(FeatureViewModel feature)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync($"api/{_endpoint}", feature);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[FeatureApiService] Create Error: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> UpdateAsync(int id, FeatureViewModel feature)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"api/{_endpoint}/{id}", feature);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[FeatureApiService] Update Error: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"api/{_endpoint}/{id}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[FeatureApiService] Delete Error: {ex.Message}");
            return false;
        }
    }
}
