using System.Net.Http.Json;

namespace Dashboard.Web.Services;

/// <summary>
/// Model yönetimi API servisi (Ürün modelleri - örn: iPhone 15, Galaxy S24)
/// </summary>
public class ModelApiService
{
    private readonly HttpClient _httpClient;
    private readonly string _endpoint = "model";

    public ModelApiService(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("ECommerceApi");
    }

    public async Task<List<ModelViewModel>?> GetAllAsync()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<List<ModelViewModel>>($"api/{_endpoint}");
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async Task<ModelViewModel?> GetByIdAsync(int id)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<ModelViewModel>($"api/{_endpoint}/{id}");
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Markaya göre modelleri getirir
    /// </summary>
    public async Task<List<ModelViewModel>?> GetByBrandIdAsync(int brandId)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<List<ModelViewModel>>($"api/{_endpoint}/brand/{brandId}");
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async Task<bool> CreateAsync(ModelViewModel model)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync($"api/{_endpoint}", model);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> UpdateAsync(int id, ModelViewModel model)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"api/{_endpoint}/{id}", model);
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
/// Model ViewModel
/// </summary>
public class ModelViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int BrandId { get; set; }
    public string? BrandName { get; set; }
    public bool IsActive { get; set; } = true;
    public int ProductCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Model oluşturma DTO
/// </summary>
public class ModelCreateViewModel
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int BrandId { get; set; }
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Model güncelleme DTO
/// </summary>
public class ModelUpdateViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int BrandId { get; set; }
    public bool IsActive { get; set; } = true;
}
