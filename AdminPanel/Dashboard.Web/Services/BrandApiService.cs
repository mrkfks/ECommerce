using System.Net.Http.Json;
using System.Text.Json;
using ECommerce.Application.DTOs;
using Microsoft.Extensions.Logging;

namespace Dashboard.Web.Services;

/// <summary>
/// Marka yönetimi API servisi
/// </summary>
public class BrandApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<BrandApiService> _logger;
    private readonly string _endpoint = "brands";

    public BrandApiService(HttpClient httpClient, ILogger<BrandApiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<List<BrandViewModel>?> GetAllAsync()
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<ApiResponseDto<List<BrandViewModel>>>($"api/{_endpoint}");
            return response?.Data;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[BrandApiService] Error: {ex.Message}");
            return null;
        }
    }

    public async Task<BrandViewModel?> GetByIdAsync(int id)
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<ApiResponseDto<BrandViewModel>>($"api/{_endpoint}/{id}");
            return response?.Data;
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> CreateAsync(BrandCreateViewModel brand)
    {
        try
        {
            _logger.LogWarning("[BrandApiService.CreateAsync] BaseAddress: {BaseAddress}", _httpClient.BaseAddress);
            _logger.LogWarning("[BrandApiService.CreateAsync] Auth header: {Auth}", _httpClient.DefaultRequestHeaders.Authorization);
            _logger.LogWarning("[BrandApiService.CreateAsync] Body: Name={Name}, Desc={Desc}, IsActive={IsActive}, CompanyId={CompanyId}",
                brand.Name, brand.Description, brand.IsActive, brand.CompanyId);
            
            // API BrandFormDto formatında gönder
            var formDto = new
            {
                Name = brand.Name,
                Description = brand.Description ?? "",
                IsActive = brand.IsActive,
                CompanyId = brand.CompanyId,
                ImageUrl = (string?)null,
                CategoryId = (int?)null
            };
            
            var json = JsonSerializer.Serialize(formDto);
            _logger.LogWarning("[BrandApiService.CreateAsync] JSON body: {Json}", json);
            
            var response = await _httpClient.PostAsJsonAsync($"api/{_endpoint}", formDto);
            _logger.LogWarning("[BrandApiService.CreateAsync] Response status: {Status}", response.StatusCode);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("[BrandApiService.CreateAsync] Error {Status}: {Error}", response.StatusCode, errorContent);
            }
            else
            {
                _logger.LogWarning("[BrandApiService.CreateAsync] SUCCESS!");
            }
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[BrandApiService.CreateAsync] Exception");
            return false;
        }
    }

    public async Task<bool> UpdateAsync(BrandUpdateViewModel brand)
    {
        try
        {
            Console.WriteLine($"[BrandApiService.UpdateAsync] Sending request to api/{_endpoint}/{brand.Id}");
            var response = await _httpClient.PutAsJsonAsync($"api/{_endpoint}/{brand.Id}", brand);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[BrandApiService.UpdateAsync] Error {response.StatusCode}: {errorContent}");
            }
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[BrandApiService.UpdateAsync] Exception: {ex.Message}\n{ex.StackTrace}");
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
        catch
        {
            return false;
        }
    }
}

/// <summary>
/// Marka ViewModel
/// </summary>
public class BrandViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public int? CompanyId { get; set; }
    public string? CompanyName { get; set; }
    public int? CategoryId { get; set; }
    public int ProductCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Marka oluşturma DTO
/// </summary>
public class BrandCreateViewModel
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public int? CompanyId { get; set; }
}

/// <summary>
/// Marka güncelleme DTO
/// </summary>
public class BrandUpdateViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Şirket ViewModel (BrandController için)
/// </summary>
public class CompanyViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public bool IsActive { get; set; } = true;
}
