using System.Net.Http.Json;
using Dashboard.Web.Services.Contracts;
using ECommerce.Application.DTOs;
using Microsoft.Extensions.Logging;

namespace Dashboard.Web.Services.Implementations;

public class TypedCategoryApiService : ICategoryApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<TypedCategoryApiService> _logger;
    private const string BaseEndpoint = "api/Category";

    public TypedCategoryApiService(HttpClient httpClient, ILogger<TypedCategoryApiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<IEnumerable<CategoryDto>> GetAllAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync(BaseEndpoint);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Kategoriler alınırken hata: {StatusCode}", response.StatusCode);
                return Enumerable.Empty<CategoryDto>();
            }

            var categories = await response.Content.ReadFromJsonAsync<List<CategoryDto>>();
            return categories ?? Enumerable.Empty<CategoryDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Kategoriler alınırken beklenmeyen hata");
            return Enumerable.Empty<CategoryDto>();
        }
    }

    public async Task<CategoryDto?> GetByIdAsync(int id)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{BaseEndpoint}/{id}");
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Kategori alınırken hata: {Id}, {StatusCode}", id, response.StatusCode);
                return null;
            }

            return await response.Content.ReadFromJsonAsync<CategoryDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Kategori alınırken beklenmeyen hata: {Id}", id);
            return null;
        }
    }

    public async Task<CategoryDto?> CreateAsync(CategoryCreateDto dto)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(BaseEndpoint, dto);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Kategori oluşturulurken hata: {StatusCode}, {Error}", 
                    response.StatusCode, errorContent);
                return null;
            }

            return await response.Content.ReadFromJsonAsync<CategoryDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Kategori oluşturulurken beklenmeyen hata");
            return null;
        }
    }

    public async Task<bool> UpdateAsync(int id, CategoryUpdateDto dto)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"{BaseEndpoint}/{id}", dto);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Kategori güncellenirken hata: {Id}, {StatusCode}, {Error}", 
                    id, response.StatusCode, errorContent);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Kategori güncellenirken beklenmeyen hata: {Id}", id);
            return false;
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"{BaseEndpoint}/{id}");
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Kategori silinirken hata: {Id}, {StatusCode}, {Error}", 
                    id, response.StatusCode, errorContent);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Kategori silinirken beklenmeyen hata: {Id}", id);
            return false;
        }
    }
}
