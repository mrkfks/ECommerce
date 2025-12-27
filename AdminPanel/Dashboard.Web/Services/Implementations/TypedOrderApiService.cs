using System.Net.Http.Json;
using Dashboard.Web.Services.Contracts;
using ECommerce.Application.DTOs;
using Microsoft.Extensions.Logging;

namespace Dashboard.Web.Services.Implementations;

public class TypedOrderApiService : IOrderApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<TypedOrderApiService> _logger;
    private const string BaseEndpoint = "api/Order";

    public TypedOrderApiService(HttpClient httpClient, ILogger<TypedOrderApiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<IEnumerable<OrderDto>> GetAllAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync(BaseEndpoint);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Siparişler alınırken hata: {StatusCode}", response.StatusCode);
                return Enumerable.Empty<OrderDto>();
            }

            var orders = await response.Content.ReadFromJsonAsync<List<OrderDto>>();
            return orders ?? Enumerable.Empty<OrderDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Siparişler alınırken beklenmeyen hata");
            return Enumerable.Empty<OrderDto>();
        }
    }

    public async Task<OrderDto?> GetByIdAsync(int id)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{BaseEndpoint}/{id}");
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Sipariş alınırken hata: {Id}, {StatusCode}", id, response.StatusCode);
                return null;
            }

            return await response.Content.ReadFromJsonAsync<OrderDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Sipariş alınırken beklenmeyen hata: {Id}", id);
            return null;
        }
    }

    public async Task<IEnumerable<OrderDto>> GetByCustomerAsync(int customerId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{BaseEndpoint}/customer/{customerId}");
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Müşteri siparişleri alınırken hata: {CustomerId}, {StatusCode}", 
                    customerId, response.StatusCode);
                return Enumerable.Empty<OrderDto>();
            }

            var orders = await response.Content.ReadFromJsonAsync<List<OrderDto>>();
            return orders ?? Enumerable.Empty<OrderDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Müşteri siparişleri alınırken hata: {CustomerId}", customerId);
            return Enumerable.Empty<OrderDto>();
        }
    }

    public async Task<bool> UpdateStatusAsync(int id, string status)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"{BaseEndpoint}/{id}/status", new { status });
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Sipariş durumu güncellenirken hata: {Id}, {StatusCode}, {Error}", 
                    id, response.StatusCode, errorContent);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Sipariş durumu güncellenirken hata: {Id}", id);
            return false;
        }
    }

    public async Task<bool> CancelAsync(int id)
    {
        try
        {
            var response = await _httpClient.PostAsync($"{BaseEndpoint}/{id}/cancel", null);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Sipariş iptal edilirken hata: {Id}, {StatusCode}, {Error}", 
                    id, response.StatusCode, errorContent);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Sipariş iptal edilirken hata: {Id}", id);
            return false;
        }
    }
}
