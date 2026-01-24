using System.Net.Http.Json;

namespace Dashboard.Web.Services;

/// <summary>
/// Marka yönetimi API servisi
/// </summary>
public class BrandApiService
{
    private readonly HttpClient _httpClient;
    private readonly string _endpoint = "brand";

    public BrandApiService(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("ECommerceApi");
    }

    public async Task<List<BrandViewModel>?> GetAllAsync()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<List<BrandViewModel>>($"api/{_endpoint}");
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
            return await _httpClient.GetFromJsonAsync<BrandViewModel>($"api/{_endpoint}/{id}");
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> CreateAsync(BrandViewModel brand)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync($"api/{_endpoint}", brand);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> UpdateAsync(int id, BrandViewModel brand)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"api/{_endpoint}/{id}", brand);
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
    public string? LogoUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public int? CompanyId { get; set; }
    public string? CompanyName { get; set; }
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
    public string? LogoUrl { get; set; }
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
    public string? LogoUrl { get; set; }
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
