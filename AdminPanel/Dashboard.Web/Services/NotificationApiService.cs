using System.Net.Http.Json;
using Dashboard.Web.Models;

namespace Dashboard.Web.Services;

/// <summary>
/// Bildirim API servisi
/// </summary>
public class NotificationApiService
{
    private readonly HttpClient _httpClient;

    public NotificationApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    /// Tüm bildirimleri getirir
    /// </summary>
    public async Task<List<NotificationDto>?> GetAllAsync()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<List<NotificationDto>>("api/notifications");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[NotificationApiService] Error: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Bildirim özeti getirir
    /// </summary>
    public async Task<NotificationSummaryVm?> GetSummaryAsync()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<NotificationSummaryVm>("api/notifications/summary");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[NotificationApiService] Error: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Okunmamış bildirimleri getirir
    /// </summary>
    public async Task<List<NotificationDto>?> GetUnreadAsync()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<List<NotificationDto>>("api/notifications/unread");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[NotificationApiService] Error: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Belirli bir bildirimi getirir
    /// </summary>
    public async Task<NotificationDto?> GetByIdAsync(int id)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<NotificationDto>($"api/Notification/{id}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[NotificationApiService] Error: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Türe göre bildirimleri getirir
    /// </summary>
    public async Task<List<NotificationDto>?> GetByTypeAsync(int type)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<List<NotificationDto>>($"api/Notification/by-type/{type}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[NotificationApiService] Error: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Düşük stoklu ürünleri getirir
    /// </summary>
    public async Task<List<LowStockItemVm>?> GetLowStockProductsAsync(int threshold = 10)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<List<LowStockItemVm>>($"api/Notification/low-stock?threshold={threshold}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[NotificationApiService] Error: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Son siparişleri getirir
    /// </summary>
    public async Task<List<RecentOrderVm>?> GetRecentOrdersAsync(int count = 10)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<List<RecentOrderVm>>($"api/Notification/recent-orders?count={count}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[NotificationApiService] Error: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Bildirimi okundu olarak işaretler
    /// </summary>
    public async Task<bool> MarkAsReadAsync(int id)
    {
        try
        {
            var response = await _httpClient.PutAsync($"api/Notification/{id}/read", null);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[NotificationApiService] Error: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Tüm bildirimleri okundu olarak işaretler
    /// </summary>
    public async Task<bool> MarkAllAsReadAsync()
    {
        try
        {
            var response = await _httpClient.PutAsync("api/Notification/read-all", null);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[NotificationApiService] Error: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Bildirimi siler
    /// </summary>
    public async Task<bool> DeleteAsync(int id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"api/Notification/{id}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[NotificationApiService] Error: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Düşük stok kontrolü yapar
    /// </summary>
    public async Task<bool> CheckLowStockAsync(int threshold = 10)
    {
        try
        {
            var response = await _httpClient.PostAsync($"api/Notification/check-low-stock?threshold={threshold}", null);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[NotificationApiService] Error: {ex.Message}");
            return false;
        }
    }
}


