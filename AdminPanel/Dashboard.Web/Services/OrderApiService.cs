using ECommerce.Application.DTOs;
using ECommerce.Domain.Enums;
using System.Net.Http.Json;

namespace Dashboard.Web.Services;

public class OrderApiService : ApiService<OrderDto>
{
    public OrderApiService(IHttpClientFactory httpClientFactory)
        : base(httpClientFactory)
    {
    }

    public async Task<bool> UpdateStatusAsync(int orderId, OrderStatus status)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"api/Order/{orderId}/status", status);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}
