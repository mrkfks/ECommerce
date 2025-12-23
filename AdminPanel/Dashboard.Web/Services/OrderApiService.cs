using ECommerce.Application.DTOs;

namespace Dashboard.Web.Services;

public class OrderApiService : ApiService<OrderDto>
{
    public OrderApiService(HttpClient httpClient) 
        : base(httpClient, "Order")
    {
    }
}
