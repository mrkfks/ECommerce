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
                var url = $"api/orders/{id}/status";
                var response = await _httpClient.PutAsJsonAsync(url, dto);
                var content = await response.Content.ReadAsStringAsync();
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception caught: {ex.Message}");
                return false;
            }
        }
    }
}
