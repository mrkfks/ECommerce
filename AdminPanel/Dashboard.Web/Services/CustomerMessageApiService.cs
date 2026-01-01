using System.Net.Http.Json;

namespace Dashboard.Web.Services;

public class CustomerMessageApiService
{
    private readonly HttpClient _httpClient;

    public CustomerMessageApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<CustomerMessageVm>> GetAllAsync()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<List<CustomerMessageVm>>("api/CustomerMessage") ?? new();
        }
        catch
        {
            return new List<CustomerMessageVm>();
        }
    }

    public async Task<List<CustomerMessageVm>> GetUnreadAsync()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<List<CustomerMessageVm>>("api/CustomerMessage/unread") ?? new();
        }
        catch
        {
            return new List<CustomerMessageVm>();
        }
    }

    public async Task<List<CustomerMessageVm>> GetPendingAsync()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<List<CustomerMessageVm>>("api/CustomerMessage/pending") ?? new();
        }
        catch
        {
            return new List<CustomerMessageVm>();
        }
    }

    public async Task<CustomerMessageSummaryVm?> GetSummaryAsync()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<CustomerMessageSummaryVm>("api/CustomerMessage/summary");
        }
        catch
        {
            return null;
        }
    }

    public async Task<CustomerMessageVm?> GetByIdAsync(int id)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<CustomerMessageVm>($"api/CustomerMessage/{id}");
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> MarkAsReadAsync(int id)
    {
        try
        {
            var response = await _httpClient.PutAsync($"api/CustomerMessage/{id}/read", null);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> ReplyAsync(int id, MessageReplyVm reply)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync($"api/CustomerMessage/{id}/reply", reply);
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
            var response = await _httpClient.DeleteAsync($"api/CustomerMessage/{id}");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}

// ViewModels
public class CustomerMessageVm
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public int CompanyId { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public bool IsReplied { get; set; }
    public string? Reply { get; set; }
    public DateTime? RepliedAt { get; set; }
    public int? RepliedByUserId { get; set; }
    public string? RepliedByName { get; set; }
    public int Category { get; set; }
    public string CategoryText { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    // UI Helpers
    public string TimeAgo
    {
        get
        {
            var diff = DateTime.Now - CreatedAt;
            if (diff.TotalMinutes < 1) return "Az önce";
            if (diff.TotalMinutes < 60) return $"{(int)diff.TotalMinutes} dk önce";
            if (diff.TotalHours < 24) return $"{(int)diff.TotalHours} saat önce";
            if (diff.TotalDays < 7) return $"{(int)diff.TotalDays} gün önce";
            return CreatedAt.ToString("dd MMM yyyy");
        }
    }

    public string StatusBadgeClass => IsReplied ? "bg-success" : (IsRead ? "bg-warning" : "bg-danger");
    public string StatusText => IsReplied ? "Yanıtlandı" : (IsRead ? "Okundu" : "Yeni");
    public string CategoryIcon => Category switch
    {
        0 => "fa-envelope", // General
        1 => "fa-shopping-cart", // Order
        2 => "fa-box", // Product
        3 => "fa-rotate-left", // Return
        4 => "fa-triangle-exclamation", // Complaint
        5 => "fa-lightbulb", // Suggestion
        _ => "fa-envelope"
    };
    public string CategoryBadgeClass => Category switch
    {
        0 => "bg-secondary",
        1 => "bg-primary",
        2 => "bg-info",
        3 => "bg-warning",
        4 => "bg-danger",
        5 => "bg-success",
        _ => "bg-secondary"
    };
    public string CustomerInitials => CustomerName.Length > 0
        ? string.Join("", CustomerName.Split(' ').Where(n => n.Length > 0).Take(2).Select(n => n[0])).ToUpper()
        : "?";
}

public class MessageReplyVm
{
    public int MessageId { get; set; }
    public string Reply { get; set; } = string.Empty;
    public int RepliedByUserId { get; set; }
}

public class CustomerMessageSummaryVm
{
    public int TotalMessages { get; set; }
    public int UnreadMessages { get; set; }
    public int PendingReplies { get; set; }
    public int RepliedMessages { get; set; }
    public List<CustomerMessageVm> RecentMessages { get; set; } = new();
}
