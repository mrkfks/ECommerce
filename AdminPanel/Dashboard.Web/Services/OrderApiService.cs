using ECommerce.Application.DTOs;

namespace Dashboard.Web.Services
{
    public class OrderApiService : ApiService<OrderDto>
    {
        public OrderApiService(HttpClient httpClient) : base(httpClient)
        {
        }
        public async Task<bool> UpdateStatusAsync(int id, ECommerce.Domain.Enums.OrderStatus status)
        {
            try
            {
                var dto = new ECommerce.Application.DTOs.UpdateOrderStatusDto { Status = status };
                var response = await _httpClient.PutAsJsonAsync($"api/Order/{id}/status", dto);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
    }
}
