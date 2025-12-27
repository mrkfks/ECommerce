using System.Net.Http.Json;
using Dashboard.Web.Services.Contracts;
using ECommerce.Application.DTOs;
using Microsoft.Extensions.Logging;

namespace Dashboard.Web.Services.Implementations;

/// <summary>
/// Typed HttpClient kullanarak tip güvenli Product API servisi
/// </summary>
public class TypedProductApiService : IProductApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<TypedProductApiService> _logger;
    private const string BaseEndpoint = "api/Product";

    public TypedProductApiService(HttpClient httpClient, ILogger<TypedProductApiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<IEnumerable<ProductDto>> GetAllAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync(BaseEndpoint);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Ürünler alınırken hata: {StatusCode}", response.StatusCode);
                return Enumerable.Empty<ProductDto>();
            }

            var products = await response.Content.ReadFromJsonAsync<List<ProductDto>>();
            return products ?? Enumerable.Empty<ProductDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ürünler alınırken beklenmeyen hata oluştu");
            return Enumerable.Empty<ProductDto>();
        }
    }

    public async Task<ProductDto?> GetByIdAsync(int id)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{BaseEndpoint}/{id}");
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Ürün alınırken hata: {Id}, {StatusCode}", id, response.StatusCode);
                return null;
            }

            return await response.Content.ReadFromJsonAsync<ProductDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ürün alınırken beklenmeyen hata: {Id}", id);
            return null;
        }
    }

    public async Task<ProductDto?> CreateAsync(ProductCreateDto dto)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(BaseEndpoint, dto);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Ürün oluşturulurken hata: {StatusCode}, {Error}", 
                    response.StatusCode, errorContent);
                return null;
            }

            return await response.Content.ReadFromJsonAsync<ProductDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ürün oluşturulurken beklenmeyen hata");
            return null;
        }
    }

    public async Task<bool> UpdateAsync(int id, ProductUpdateDto dto)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"{BaseEndpoint}/{id}", dto);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Ürün güncellenirken hata: {Id}, {StatusCode}, {Error}", 
                    id, response.StatusCode, errorContent);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ürün güncellenirken beklenmeyen hata: {Id}", id);
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
                _logger.LogWarning("Ürün silinirken hata: {Id}, {StatusCode}, {Error}", 
                    id, response.StatusCode, errorContent);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ürün silinirken beklenmeyen hata: {Id}", id);
            return false;
        }
    }

    public async Task<IEnumerable<ProductDto>> GetByCategoryAsync(int categoryId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{BaseEndpoint}/category/{categoryId}");
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Kategoriye göre ürünler alınırken hata: {CategoryId}, {StatusCode}", 
                    categoryId, response.StatusCode);
                return Enumerable.Empty<ProductDto>();
            }

            var products = await response.Content.ReadFromJsonAsync<List<ProductDto>>();
            return products ?? Enumerable.Empty<ProductDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Kategoriye göre ürünler alınırken hata: {CategoryId}", categoryId);
            return Enumerable.Empty<ProductDto>();
        }
    }

    public async Task<IEnumerable<ProductDto>> GetByBrandAsync(int brandId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{BaseEndpoint}/brand/{brandId}");
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Markaya göre ürünler alınırken hata: {BrandId}, {StatusCode}", 
                    brandId, response.StatusCode);
                return Enumerable.Empty<ProductDto>();
            }

            var products = await response.Content.ReadFromJsonAsync<List<ProductDto>>();
            return products ?? Enumerable.Empty<ProductDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Markaya göre ürünler alınırken hata: {BrandId}", brandId);
            return Enumerable.Empty<ProductDto>();
        }
    }
}
