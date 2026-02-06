using System.Net.Http.Json;
using System.Text.Json;
using Dashboard.Web.Models;
using ECommerce.Application.DTOs;

namespace Dashboard.Web.Services;

public class CampaignApiService
{
    private readonly HttpClient _httpClient;

    public CampaignApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<CampaignVm>> GetAllAsync()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<List<CampaignVm>>("api/Campaign") ?? new();
        }
        catch
        {
            return new List<CampaignVm>();
        }
    }

    public async Task<List<CampaignVm>> GetActiveAsync()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<List<CampaignVm>>("api/Campaign/active") ?? new();
        }
        catch
        {
            return new List<CampaignVm>();
        }
    }

    public async Task<CampaignSummaryVm?> GetSummaryAsync()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<CampaignSummaryVm>("api/Campaign/summary");
        }
        catch
        {
            return null;
        }
    }

    public async Task<CampaignVm?> GetByIdAsync(int id)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<CampaignVm>($"api/Campaign/{id}");
        }
        catch
        {
            return null;
        }
    }

    public async Task<(bool Success, int? Id)> CreateAsync(CampaignCreateVm campaign)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/Campaign", campaign);
            if (response.IsSuccessStatusCode)
            {
                var jsonStr = await response.Content.ReadAsStringAsync();
                using (JsonDocument doc = JsonDocument.Parse(jsonStr))
                {
                    if (doc.RootElement.TryGetProperty("id", out var idElement) && int.TryParse(idElement.GetString(), out int id))
                    {
                        return (true, id);
                    }
                }
                return (true, null);
            }
            return (false, null);
        }
        catch
        {
            return (false, null);
        }
    }

    public async Task<bool> UpdateAsync(int id, CampaignUpdateVm campaign)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"api/Campaign/{id}", campaign);
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
            var response = await _httpClient.PutAsync($"api/Campaign/{id}/activate", null);
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
            var response = await _httpClient.PutAsync($"api/Campaign/{id}/deactivate", null);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"api/campaigns/{id}");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    // ProductCampaign Methods

    public async Task<List<ProductCampaignVm>> GetCampaignProductsAsync(int campaignId)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<List<ProductCampaignVm>>($"api/campaigns/{campaignId}/products") ?? new();
        }
        catch
        {
            return new List<ProductCampaignVm>();
        }
    }

    public async Task<bool> AddProductToCampaignAsync(int campaignId, ProductCampaignCreateVm product)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync($"api/campaigns/{campaignId}/products", product);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> RemoveProductFromCampaignAsync(int campaignId, int productId)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"api/campaigns/{campaignId}/products/{productId}");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> UpdateProductCampaignPricesAsync(int campaignId, int productId, ProductCampaignCreateVm prices)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"api/campaigns/{campaignId}/products/{productId}", prices);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<(bool Success, string? ImageUrl)> UploadBannerAsync(int campaignId, IFormFile file)
    {
        try
        {
            using (var content = new MultipartFormDataContent())
            {
                using (var fileStream = file.OpenReadStream())
                {
                    var streamContent = new StreamContent(fileStream);
                    streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType ?? "application/octet-stream");
                    content.Add(streamContent, "file", file.FileName);

                    var response = await _httpClient.PutAsync($"api/campaigns/{campaignId}/upload-banner", content);

                    if (response.IsSuccessStatusCode)
                    {
                        var jsonStr = await response.Content.ReadAsStringAsync();
                        using (JsonDocument doc = JsonDocument.Parse(jsonStr))
                        {
                            if (doc.RootElement.TryGetProperty("url", out var urlElement))
                            {
                                return (true, urlElement.GetString());
                            }
                        }
                        return (true, null);
                    }
                    return (false, null);
                }
            }
        }
        catch
        {
            return (false, null);
        }
    }
}
