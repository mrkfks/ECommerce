using System.Net.Http.Json;
using Dashboard.Web.Models;
using ECommerce.Application.DTOs;
using Microsoft.Extensions.Logging;

namespace Dashboard.Web.Services;

/// <summary>
/// Özellik yönetimi API servisi
/// </summary>
public class FeatureApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<FeatureApiService> _logger;
    private readonly string _endpoint = "global-attributes";

    public FeatureApiService(IHttpClientFactory httpClientFactory, ILogger<FeatureApiService> logger)
    {
        _httpClient = httpClientFactory.CreateClient("ECommerceApi");
        _logger = logger;
    }

    public async Task<List<FeatureViewModel>?> GetAllAsync()
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<ApiResponseDto<List<FeatureViewModel>>>($"api/{_endpoint}");
            return response?.Data;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[FeatureApiService] GetAll Error");
            return null;
        }
    }

    public async Task<FeatureViewModel?> GetByIdAsync(int id)
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<ApiResponseDto<FeatureViewModel>>($"api/{_endpoint}/{id}");
            return response?.Data;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[FeatureApiService] GetById Error for id={Id}", id);
            return null;
        }
    }

    public async Task<bool> CreateAsync(FeatureViewModel feature)
    {
        try
        {
            // API GlobalAttributeFormDto formatında gönder
            var formDto = new
            {
                Name = feature.Name,
                DisplayName = feature.Name,
                Description = feature.Description ?? "",
                AttributeType = "Text",
                DisplayOrder = feature.DisplayOrder,
                IsActive = feature.IsActive,
                Values = feature.Values?.Select(v => new
                {
                    Value = v.Value,
                    DisplayValue = v.Value,
                    DisplayOrder = v.DisplayOrder,
                    IsActive = v.IsActive
                }).ToList()
            };

            _logger.LogInformation("[FeatureApiService] Creating feature: {Name}", feature.Name);
            var response = await _httpClient.PostAsJsonAsync($"api/{_endpoint}", formDto);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("[FeatureApiService] Create Error {Status}: {Error}", response.StatusCode, error);
            }

            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[FeatureApiService] Create Exception");
            return false;
        }
    }

    public async Task<bool> UpdateAsync(int id, FeatureViewModel feature)
    {
        try
        {
            var formDto = new
            {
                Id = id,
                Name = feature.Name,
                DisplayName = feature.Name,
                Description = feature.Description ?? "",
                AttributeType = "Text",
                DisplayOrder = feature.DisplayOrder,
                IsActive = feature.IsActive,
                Values = feature.Values?.Select(v => new
                {
                    Id = v.Id > 0 ? (int?)v.Id : null,
                    Value = v.Value,
                    DisplayValue = v.Value,
                    DisplayOrder = v.DisplayOrder,
                    IsActive = v.IsActive
                }).ToList()
            };

            var response = await _httpClient.PutAsJsonAsync($"api/{_endpoint}/{id}", formDto);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[FeatureApiService] Update Error");
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
            _logger.LogError(ex, "[FeatureApiService] Delete Error");
            return false;
        }
    }
}
