using Dashboard.Web.Models;
using System.Net.Http.Json;
using System.Net.Http.Json;
using ECommerce.Application.DTOs;

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


