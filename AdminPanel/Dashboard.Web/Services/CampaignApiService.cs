using System.Net.Http.Json;
using System.Text.Json;
using Dashboard.Web.Models;
using ECommerce.Application.DTOs;
using Microsoft.Extensions.Logging;

namespace Dashboard.Web.Services;

public class CampaignApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CampaignApiService> _logger;
    private static readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public CampaignApiService(HttpClient httpClient, ILogger<CampaignApiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    /// <summary>
    /// API yanıtından data alanını çıkarır. API yanıtları {"success":true,"data":...,"message":""} formatında döner.
    /// </summary>
    private T? ExtractData<T>(string json)
    {
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        // Sarmalayıcı format: {"success":true,"data":[...],"message":""}
        if (root.TryGetProperty("data", out var dataElement))
        {
            var dataJson = dataElement.GetRawText();
            return JsonSerializer.Deserialize<T>(dataJson, _jsonOptions);
        }

        // Doğrudan data dene (sarmalayıcı yoksa)
        return JsonSerializer.Deserialize<T>(json, _jsonOptions);
    }

    public async Task<List<CampaignVm>> GetAllAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("api/campaigns");
            var responseBody = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("[CampaignApiService.GetAllAsync] Status: {Status}", response.StatusCode);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("[CampaignApiService.GetAllAsync] API error {Status}: {Body}", response.StatusCode, responseBody);
                return new List<CampaignVm>();
            }

            var campaigns = ExtractData<List<CampaignVm>>(responseBody!);
            _logger.LogInformation("[CampaignApiService.GetAllAsync] Deserialized {Count} campaigns", campaigns?.Count ?? 0);
            return campaigns ?? new();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[CampaignApiService.GetAllAsync] Exception");
            return new List<CampaignVm>();
        }
    }

    public async Task<List<CampaignVm>> GetActiveAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("api/campaigns/active");
            if (!response.IsSuccessStatusCode) return new List<CampaignVm>();
            var body = await response.Content.ReadAsStringAsync();
            return ExtractData<List<CampaignVm>>(body) ?? new();
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
            var response = await _httpClient.GetAsync("api/campaigns/summary");
            if (!response.IsSuccessStatusCode) return null;
            var body = await response.Content.ReadAsStringAsync();
            return ExtractData<CampaignSummaryVm>(body);
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
            var response = await _httpClient.GetAsync($"api/campaigns/{id}");
            if (!response.IsSuccessStatusCode) return null;
            var body = await response.Content.ReadAsStringAsync();
            return ExtractData<CampaignVm>(body);
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
            _logger.LogInformation("[CampaignApiService.CreateAsync] Sending: Name={Name}, Discount={Discount}",
                campaign.Name, campaign.DiscountPercent);

            var response = await _httpClient.PostAsJsonAsync("api/campaigns", campaign);
            var responseBody = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("[CampaignApiService.CreateAsync] Response: {Status} - {Body}", response.StatusCode, responseBody);

            if (response.IsSuccessStatusCode)
            {
                using var doc = JsonDocument.Parse(responseBody);
                var root = doc.RootElement;

                // Sarmalayıcı yanıt: {"success":true,"data":{"id":1,"message":"..."},"message":""}
                var dataElement = root.TryGetProperty("data", out var d) ? d : root;

                if (dataElement.TryGetProperty("id", out var idElement))
                {
                    int? campaignId = idElement.ValueKind == JsonValueKind.Number
                        ? idElement.GetInt32()
                        : (int.TryParse(idElement.GetString(), out int parsed) ? parsed : null);
                    _logger.LogInformation("[CampaignApiService.CreateAsync] Campaign created ID: {Id}", campaignId);
                    return (true, campaignId);
                }
                return (true, null);
            }

            _logger.LogWarning("[CampaignApiService.CreateAsync] Error: {Status} - {Body}", response.StatusCode, responseBody);
            return (false, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[CampaignApiService.CreateAsync] Exception");
            return (false, null);
        }
    }

    public async Task<bool> UpdateAsync(int id, CampaignUpdateVm campaign)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"api/campaigns/{id}", campaign);
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
            var response = await _httpClient.PutAsync($"api/campaigns/{id}/activate", null);
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
            var response = await _httpClient.PutAsync($"api/campaigns/{id}/deactivate", null);
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
            _logger.LogInformation("[CampaignApiService.DeleteAsync] Deleting campaign ID: {CampaignId}", id);
            var response = await _httpClient.DeleteAsync($"api/campaigns/{id}");
            var responseBody = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("[CampaignApiService.DeleteAsync] Status: {Status}, Response: {Response}",
                response.StatusCode, responseBody);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("[CampaignApiService.DeleteAsync] Delete failed for ID: {CampaignId} - Status: {Status} - Body: {Body}",
                    id, response.StatusCode, responseBody);
            }

            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[CampaignApiService.DeleteAsync] Exception deleting campaign ID: {CampaignId}", id);
            return false;
        }
    }

    // ProductCampaign Methods

    public async Task<List<ProductCampaignVm>> GetCampaignProductsAsync(int campaignId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/campaigns/{campaignId}/products");
            var responseBody = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("[CampaignApiService.GetCampaignProductsAsync] CampaignId: {CampaignId}, Status: {Status}", campaignId, response.StatusCode);
            _logger.LogInformation("[CampaignApiService.GetCampaignProductsAsync] Response: {Response}", responseBody);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("[CampaignApiService.GetCampaignProductsAsync] HTTP Error: {Status} - {Response}", response.StatusCode, responseBody);
                return new();
            }

            return ExtractData<List<ProductCampaignVm>>(responseBody) ?? new();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[CampaignApiService.GetCampaignProductsAsync] Exception for campaignId: {CampaignId}", campaignId);
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
                            var dataElement = doc.RootElement.TryGetProperty("data", out var d) ? d : doc.RootElement;
                            if (dataElement.TryGetProperty("url", out var urlElement))
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

    // Category-based Campaign Methods

    public async Task<List<CategorySelectionVm>> GetAllCategoriesAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("api/categories");
            if (!response.IsSuccessStatusCode) return new();

            var body = await response.Content.ReadAsStringAsync();
            var categories = ExtractData<List<CategoryDto>>(body) ?? new();

            // Convert to CategorySelectionVm
            return categories.Select(c => new CategorySelectionVm
            {
                Id = c.Id,
                Name = c.Name,
                ProductCount = c.ProductCount ?? 0
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[CampaignApiService.GetAllCategoriesAsync] Exception");
            return new();
        }
    }

    public async Task<List<int>> GetCampaignCategoriesAsync(int campaignId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/campaigns/{campaignId}/categories");
            if (!response.IsSuccessStatusCode) return new();

            var body = await response.Content.ReadAsStringAsync();
            return ExtractData<List<int>>(body) ?? new();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[CampaignApiService.GetCampaignCategoriesAsync] Exception for campaignId: {CampaignId}", campaignId);
            return new();
        }
    }
}
