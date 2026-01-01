using System.Net.Http.Json;

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
    public async Task<List<NotificationVm>?> GetAllAsync()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<List<NotificationVm>>("api/Notification");
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
            return await _httpClient.GetFromJsonAsync<NotificationSummaryVm>("api/Notification/summary");
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
    public async Task<List<NotificationVm>?> GetUnreadAsync()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<List<NotificationVm>>("api/Notification/unread");
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
    public async Task<NotificationVm?> GetByIdAsync(int id)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<NotificationVm>($"api/Notification/{id}");
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
    public async Task<List<NotificationVm>?> GetByTypeAsync(int type)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<List<NotificationVm>>($"api/Notification/by-type/{type}");
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

#region ViewModels

/// <summary>
/// Bildirim ViewModel
/// </summary>
public class NotificationVm
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public int Type { get; set; }
    public string TypeText { get; set; } = string.Empty;
    public int Priority { get; set; }
    public string PriorityText { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? EntityType { get; set; }
    public int? EntityId { get; set; }
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
    public string? Data { get; set; }
    public string? ActionUrl { get; set; }
    public string? ActionText { get; set; }
    public DateTime CreatedAt { get; set; }
    public string TimeAgo { get; set; } = string.Empty;

    // UI helpers
    public string TypeIcon => Type switch
    {
        1 => "fa-box-open",      // LowStock
        2 => "fa-shopping-cart", // NewOrder
        3 => "fa-undo",          // ReturnRequest
        4 => "fa-credit-card",   // PaymentFailed
        5 => "fa-truck",         // OrderStatusChanged
        _ => "fa-bell"           // System
    };

    public string TypeColor => Type switch
    {
        1 => "danger",   // LowStock - Kritik
        2 => "success",  // NewOrder - Bilgi
        3 => "warning",  // ReturnRequest - Uyarı
        4 => "danger",   // PaymentFailed - Kritik
        5 => "info",     // OrderStatusChanged - Bilgi
        _ => "secondary" // System
    };

    public string PriorityColor => Priority switch
    {
        1 => "secondary", // Low
        2 => "info",      // Normal
        3 => "warning",   // High
        4 => "danger",    // Critical
        _ => "secondary"
    };
}

/// <summary>
/// Bildirim özeti ViewModel
/// </summary>
public class NotificationSummaryVm
{
    public int TotalCount { get; set; }
    public int UnreadCount { get; set; }
    public int LowStockCount { get; set; }
    public int NewOrderCount { get; set; }
    public int ReturnRequestCount { get; set; }
    public int PaymentFailedCount { get; set; }
    public List<NotificationVm> RecentNotifications { get; set; } = new();
}

/// <summary>
/// Düşük stok ürün ViewModel
/// </summary>
public class LowStockItemVm
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public int CurrentStock { get; set; }
    public int Threshold { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string BrandName { get; set; } = string.Empty;
    public decimal Price { get; set; }

    public string StockStatusColor => CurrentStock == 0 ? "danger" : CurrentStock < 5 ? "warning" : "info";
    public string StockStatusText => CurrentStock == 0 ? "Stokta Yok" : CurrentStock < 5 ? "Kritik" : "Düşük";
}

/// <summary>
/// Son sipariş ViewModel
/// </summary>
public class RecentOrderVm
{
    public int OrderId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public int ItemCount { get; set; }
    public DateTime OrderDate { get; set; }
    public string TimeAgo { get; set; } = string.Empty;
}

#endregion
