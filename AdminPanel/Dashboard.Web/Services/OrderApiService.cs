using ECommerce.Application.DTOs;
using ECommerce.Domain.Enums;
using System.Net.Http.Json;

namespace Dashboard.Web.Services;

public class OrderApiService : ApiService<OrderDto>
{
    public OrderApiService(HttpClient httpClient)
        : base(httpClient, "Order")
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
