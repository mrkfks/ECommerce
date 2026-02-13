using System.Net.Http.Json;

namespace Dashboard.Web.Services;

/// <summary>
/// Global Attribute (Site geneli ayarlar) API servisi
/// </summary>
public class GlobalAttributeApiService
{
    private readonly HttpClient _httpClient;
    private readonly string _endpoint = "global-attributes";

    public GlobalAttributeApiService(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("ECommerceApi");
    }

    public async Task<List<GlobalAttributeViewModel>?> GetAllAsync()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<List<GlobalAttributeViewModel>>($"api/{_endpoint}");
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async Task<GlobalAttributeViewModel?> GetByIdAsync(int id)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<GlobalAttributeViewModel>($"api/{_endpoint}/{id}");
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> CreateAsync(ECommerce.Application.DTOs.GlobalAttributeFormDto attribute)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync($"api/{_endpoint}", attribute);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> UpdateAsync(int id, GlobalAttributeViewModel attribute)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"api/{_endpoint}/{id}", attribute);
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
/// Global Attribute ViewModel
/// </summary>
public class GlobalAttributeViewModel
{
    public int Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Group { get; set; }
    public int? CompanyId { get; set; }
    public string? CompanyName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Global Attribute oluşturma DTO
/// </summary>
public class GlobalAttributeCreateViewModel
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Group { get; set; }
    public int? CompanyId { get; set; }

    // Eksik property'ler eklendi
    public string? DisplayName { get; set; }
    public string? AttributeType { get; set; }
    public int? DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public List<GlobalAttributeValueCreateViewModel>? Values { get; set; }
}

public class GlobalAttributeValueCreateViewModel
{
    public string Value { get; set; } = string.Empty;
    public string? DisplayValue { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public string? ColorCode { get; set; }
}

/// <summary>
/// Global Attribute güncelleme DTO
/// </summary>
public class GlobalAttributeUpdateViewModel
{
    public int Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Group { get; set; }
}
