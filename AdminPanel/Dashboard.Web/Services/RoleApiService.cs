using System.Net.Http.Json;

namespace Dashboard.Web.Services;

/// <summary>
/// Rol yönetimi API servisi
/// </summary>
public class RoleApiService
{
    private readonly HttpClient _httpClient;

    public RoleApiService(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("ECommerceApi");
    }

    /// <summary>
    /// Tüm rolleri getirir
    /// </summary>
    public async Task<List<RoleViewModel>?> GetAllAsync()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<List<RoleViewModel>>("api/role");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception caught: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// ID'ye göre rol getirir
    /// </summary>
    public async Task<RoleViewModel?> GetByIdAsync(int id)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<RoleViewModel>($"api/role/{id}");
        }
        catch (Exception)
        {
            return null;
        }
    }
}

/// <summary>
/// Rol ViewModel
/// </summary>
public class RoleViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsSystem { get; set; }
    public int UserCount { get; set; }
    public DateTime CreatedAt { get; set; }
}
