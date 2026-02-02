using Dashboard.Web.Models;
using System.Net.Http.Json;
using System.Net.Http.Json;
using ECommerce.Application.DTOs;

namespace Dashboard.Web.Services;

public class CampaignApiService
{
    private readonly HttpClient _httpClient;

    public CampaignApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<CampaignVm>> GetAllAsync()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<List<CampaignVm>>("api/Campaign") ?? new();
        }
        catch
        {
            return new List<CampaignVm>();
        }
    }

    public async Task<List<CampaignVm>> GetActiveAsync()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<List<CampaignVm>>("api/Campaign/active") ?? new();
        }
        catch
        {
            return new List<CampaignVm>();
        }
    }

    public async Task<CampaignSummaryVm?> GetSummaryAsync()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<CampaignSummaryVm>("api/Campaign/summary");
        }
        catch
        {
            return null;
        }
    }

    public async Task<CampaignVm?> GetByIdAsync(int id)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<CampaignVm>($"api/Campaign/{id}");
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> CreateAsync(CampaignCreateVm campaign)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/Campaign", campaign);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> UpdateAsync(int id, CampaignUpdateVm campaign)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"api/Campaign/{id}", campaign);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> ActivateAsync(int id)
    {
        try
        {
            var response = await _httpClient.PutAsync($"api/Campaign/{id}/activate", null);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> DeactivateAsync(int id)
    {
        try
        {
            var response = await _httpClient.PutAsync($"api/Campaign/{id}/deactivate", null);
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
            var response = await _httpClient.DeleteAsync($"api/Campaign/{id}");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}

// ViewModels
public class CampaignVm
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal DiscountPercent { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; }
    public bool IsCurrentlyActive { get; set; }
    public int RemainingDays { get; set; }
    public int CompanyId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    // UI Helpers
    public string DiscountText => $"%{DiscountPercent:N0}";
    public string DateRangeText => $"{StartDate:dd MMM} - {EndDate:dd MMM yyyy}";
    public string StatusBadgeClass => IsCurrentlyActive ? "bg-success" : (StartDate > DateTime.Now ? "bg-info" : "bg-secondary");
    public string StatusText => IsCurrentlyActive ? "Aktif" : (StartDate > DateTime.Now ? "Yaklaşan" : "Sona Erdi");
    public string RemainingDaysText => RemainingDays > 0 ? $"{RemainingDays} gün kaldı" : "Sona erdi";
}


public class CampaignSummaryVm
{
    public int TotalCampaigns { get; set; }
    public int ActiveCampaigns { get; set; }
    public int UpcomingCampaigns { get; set; }
    public int ExpiredCampaigns { get; set; }
    public List<CampaignVm> CurrentCampaigns { get; set; } = new();
}
