using System.Net.Http.Json;
using Dashboard.Web.Models;
using ECommerce.Application.DTOs;
using ECommerce.Application.Responses;

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
            var response = await _httpClient.GetFromJsonAsync<ApiResponse<List<NotificationDto>>>("api/notifications");
            return response?.Data;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[NotificationApiService] GetAllAsync Error: {ex.Message}");
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
            var response = await _httpClient.GetFromJsonAsync<ApiResponse<NotificationSummaryDto>>("api/notifications/summary");
            if (response?.Data != null)
            {
                return new NotificationSummaryVm
                {
                    TotalCount = response.Data.TotalCount,
                    UnreadCount = response.Data.UnreadCount,
                    LowStockCount = response.Data.LowStockCount,
                    NewOrderCount = response.Data.NewOrderCount,
                    ReturnRequestCount = response.Data.ReturnRequestCount,
                    PaymentFailedCount = response.Data.PaymentFailedCount
                };
            }
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[NotificationApiService] GetSummaryAsync Error: {ex.Message}");
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
            var response = await _httpClient.GetFromJsonAsync<ApiResponse<List<NotificationDto>>>("api/notifications/unread");
            return response?.Data;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[NotificationApiService] GetUnreadAsync Error: {ex.Message}");
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
            var response = await _httpClient.GetFromJsonAsync<ApiResponse<NotificationDto>>($"api/notifications/{id}");
            return response?.Data;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[NotificationApiService] GetByIdAsync Error: {ex.Message}");
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
            var response = await _httpClient.GetFromJsonAsync<ApiResponse<List<NotificationDto>>>($"api/notifications/by-type/{type}");
            return response?.Data;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[NotificationApiService] GetByTypeAsync Error: {ex.Message}");
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
            var response = await _httpClient.GetFromJsonAsync<ApiResponse<List<LowStockProductDto>>>($"api/notifications/low-stock?threshold={threshold}");
            if (response?.Data != null)
            {
                return response.Data.Select(p => new LowStockItemVm
                {
                    ProductId = p.ProductId,
                    ProductName = p.ProductName,
                    CurrentStock = p.CurrentStock,
                    Threshold = p.Threshold
                }).ToList();
            }
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[NotificationApiService] GetLowStockProductsAsync Error: {ex.Message}");
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
            var response = await _httpClient.GetFromJsonAsync<ApiResponse<List<NewOrderNotificationDto>>>($"api/notifications/recent-orders?count={count}");
            if (response?.Data != null)
            {
                return response.Data.Select(o => new RecentOrderVm
                {
                    OrderId = o.OrderId,
                    CustomerName = o.CustomerName,
                    TotalAmount = o.TotalAmount,
                    OrderDate = o.OrderDate
                }).ToList();
            }
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[NotificationApiService] GetRecentOrdersAsync Error: {ex.Message}");
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
            var response = await _httpClient.PutAsync($"api/notifications/{id}/read", null);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[NotificationApiService] MarkAsReadAsync Error: {ex.Message}");
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
            var response = await _httpClient.PutAsync("api/notifications/read-all", null);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[NotificationApiService] MarkAllAsReadAsync Error: {ex.Message}");
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
            var response = await _httpClient.DeleteAsync($"api/notifications/{id}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[NotificationApiService] DeleteAsync Error: {ex.Message}");
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
            var response = await _httpClient.PostAsync($"api/notifications/check-low-stock?threshold={threshold}", null);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[NotificationApiService] CheckLowStockAsync Error: {ex.Message}");
            return false;
        }
    }
}


